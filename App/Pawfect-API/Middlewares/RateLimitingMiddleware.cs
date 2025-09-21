using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Pawfect_API.DevTools;
using Pawfect_API.Attributes;
using Pawfect_API.Data.Entities.EnumTypes;
using Pawfect_API.Data.Entities.Types.RateLimiting;
using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using System.Security.Claims;

namespace Pawfect_API.Middlewares
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly RateLimitConfig _config;
        private readonly ILogger<RateLimitingMiddleware> _logger;

        private static readonly ConcurrentDictionary<String, SemaphoreSlim> _keyLocks = new();

        public RateLimitingMiddleware(
            RequestDelegate next,
            IMemoryCache cache,
            IOptions<RateLimitConfig> config,
            ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _cache = cache;
            _config = config.Value;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip rate limiting for certain endpoints (health checks, swagger, etc.)
            if (this.ShouldSkipRateLimit(context))
            {
                await _next(context);
                return;
            }

            Endpoint endpoint = context.GetEndpoint();
            if (endpoint == null)
            {
                await _next(context);
                return;
            }

            (RateLimitLevel Level, RateLimitTier Tier, String CustomKey)? rateLimitInfo = this.GetRateLimitInfo(endpoint);
            if (rateLimitInfo == null)
            {
                await _next(context);
                return;
            }

            String clientId = this.GetClientIdentifier(context);
            String rateLimitKey = this.GenerateRateLimitKey(clientId, rateLimitInfo.Value.CustomKey ?? endpoint.DisplayName);

            if (!await IsRequestAllowed(rateLimitKey, rateLimitInfo.Value.Tier))
            {
                await HandleRateLimitExceeded(context, rateLimitInfo.Value.Tier);
                return;
            }

            await _next(context);
        }

        private bool ShouldSkipRateLimit(HttpContext context)
        {
            String path = context.Request.Path.Value?.ToLower();
            return path != null && (
                path.StartsWith("/swagger") ||
                path.StartsWith("/health") ||
                path.StartsWith("/_framework") ||
                path.EndsWith(".js") ||
                path.EndsWith(".css") ||
                path.EndsWith(".ico"));
        }

        private (RateLimitLevel Level, RateLimitTier Tier, String? CustomKey)? GetRateLimitInfo(Endpoint endpoint)
        {
            // Check for method-level attribute first
            if (endpoint.Metadata.GetMetadata<ControllerActionDescriptor>() is ControllerActionDescriptor actionDescriptor)
            {
                RateLimitAttribute methodAttribute = actionDescriptor.MethodInfo.GetCustomAttribute<RateLimitAttribute>();
                if (methodAttribute != null)
                {
                    return (methodAttribute.Level, GetTierConfig(methodAttribute.Level), methodAttribute.CustomKey);
                }

                // Check for controller-level attribute
                RateLimitAttribute controllerAttribute = actionDescriptor.ControllerTypeInfo.GetCustomAttribute<RateLimitAttribute>();
                if (controllerAttribute != null)
                {
                    return (controllerAttribute.Level, GetTierConfig(controllerAttribute.Level), controllerAttribute.CustomKey);
                }
            }

            // Default to moderate level if no attribute is found
            return (RateLimitLevel.Moderate, GetTierConfig(RateLimitLevel.Moderate), null);
        }

        private RateLimitTier GetTierConfig(RateLimitLevel level)
        {
            return level switch
            {
                RateLimitLevel.Permissive => _config.Permissive,
                RateLimitLevel.Moderate => _config.Moderate,
                RateLimitLevel.Restrictive => _config.Restrictive,
                RateLimitLevel.Strict => _config.Strict,
                _ => _config.Moderate
            };
        }

        private String GetClientIdentifier(HttpContext context)
        {
            // Try to get user ID from JWT claims first
            String userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!String.IsNullOrEmpty(userId))
                return $"user:{userId}";

            // Fall back to IP address
            String ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return $"ip:{ipAddress}";
        }

        private String GenerateRateLimitKey(String clientId, String? endpoint)
        {
            return $"ratelimit:{clientId}:{endpoint ?? "global"}";
        }

        private async Task<bool> IsRequestAllowed(String key, RateLimitTier tier)
        {
            if (!tier.Enabled)
                return true;

            String lockKey = $"lock:{key}";
            SemaphoreSlim semaphore = _keyLocks.GetOrAdd(lockKey, _ => new SemaphoreSlim(1, 1));

            await semaphore.WaitAsync();
            try
            {
                DateTime now = DateTime.UtcNow;
                DateTime windowStart = now.Subtract(tier.WindowSize);

                if (!_cache.TryGetValue(key, out List<DateTime>? timestamps))
                {
                    timestamps = new List<DateTime>();
                }

                // Remove expired timestamps
                timestamps = timestamps.Where(t => t > windowStart).ToList();

                // Check if we can add a new request
                if (timestamps.Count >= tier.RequestsPerMinute)
                {
                    _logger.LogWarning("Rate limit exceeded for key: {Key}. Current count: {Count}, Limit: {Limit}", 
                        key, timestamps.Count, tier.RequestsPerMinute);
                    return false;
                }

                // Add current timestamp
                timestamps.Add(now);

                // Cache with appropriate expiration
                MemoryCacheEntryOptions cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = tier.WindowSize.Add(TimeSpan.FromMinutes(1)),
                    SlidingExpiration = tier.WindowSize
                };

                _cache.Set(key, timestamps, cacheOptions);
                return true;
            }
            finally
            {
                semaphore.Release();
            }
        }

        private async Task HandleRateLimitExceeded(HttpContext context, RateLimitTier tier)
        {
            context.Response.StatusCode = (int) HttpStatusCode.TooManyRequests;
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = "Rate limit exceeded",
                message = $"Too many requests. Limit: {tier.RequestsPerMinute} requests per minute.",
                retryAfter = tier.WindowSize.TotalSeconds
            };

            // Add rate limit headers
            context.Response.Headers.Append("X-RateLimit-Limit", tier.RequestsPerMinute.ToString());
            context.Response.Headers.Append("X-RateLimit-Remaining", "0");
            context.Response.Headers.Append("X-RateLimit-Reset", DateTimeOffset.UtcNow.Add(tier.WindowSize).ToUnixTimeSeconds().ToString());
            context.Response.Headers.Append("Retry-After", ((int)tier.WindowSize.TotalSeconds).ToString());

            String jsonResponse = JsonHelper.SerializeObjectFormattedSafe(response);
            await context.Response.WriteAsync(jsonResponse);
        }
    }

    public static class RateLimitMiddlewareExtensions
    {
        public static IApplicationBuilder UseRateLimitMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RateLimitingMiddleware>();
        }
    }
}
