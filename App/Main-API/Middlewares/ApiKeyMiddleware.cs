using Main_API.Middlewares;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Apis;

namespace Pawfect_Pet_Adoption_App_API.Middlewares
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ApiKeyConfig _config;

        private const String ApiKeyHeaderName = "ApiKey";

        public ApiKeyMiddleware
        (
            RequestDelegate next,
            IOptions<ApiKeyConfig> config
        )
        {
            _next = next;
            _config = config.Value;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only check API key for routes starting with /api
            if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
            {
                if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out StringValues headerValue))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("API Key is missing.");
                    return;
                }

                String providedApiKey = headerValue.ToString().Replace("ApiKey: ", "").Trim();

                if (!_config.KeyRecords.Any(keyRecord => keyRecord.ApiKey.Equals(providedApiKey)))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync("Invalid API Key.");
                    return;
                }
            }

            await _next(context);
        }
    }

    public static class ApiKeyMiddlewareExtensions
    {
        public static IApplicationBuilder UseApiKeyMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiKeyMiddleware>();
        }
    }
}
