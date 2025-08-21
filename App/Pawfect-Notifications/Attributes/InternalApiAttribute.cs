using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using Pawfect_Pet_Adoption_App_Notifications.Data.Entities.Types.Authorisation;

namespace Pawfect_Notifications.Attributes
{
    public class InternalApiAttribute : ActionFilterAttribute
    {
        private readonly InternalApiConfig _config;
        private readonly ILogger<InternalApiAttribute> _logger;

        public InternalApiAttribute(IOptions<InternalApiConfig> config, ILogger<InternalApiAttribute> logger)
        {
            _config = config.Value;
            _logger = logger;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            HttpRequest request = context.HttpContext.Request;

            // Check for internal API headers
            if (!request.Headers.TryGetValue("X-Internal-Service", out Microsoft.Extensions.Primitives.StringValues serviceHeader) ||
                !request.Headers.TryGetValue("X-Internal-Signature", out Microsoft.Extensions.Primitives.StringValues signatureHeader) ||
                !request.Headers.TryGetValue("X-Internal-Timestamp", out Microsoft.Extensions.Primitives.StringValues timestampHeader))
            {
                _logger.LogWarning("Missing internal API headers from {RemoteIp}", this.GetClientIP(context.HttpContext));
                context.Result = new UnauthorizedResult();
                return;
            }

            String serviceName = serviceHeader.ToString();
            String signature = signatureHeader.ToString();
            String timestamp = timestampHeader.ToString();

            // Validate service name
            if (!_config.AllowedServices.Contains(serviceName))
            {
                _logger.LogWarning("Unauthorized service '{ServiceName}' attempted access", serviceName);
                context.Result = new UnauthorizedResult();
                return;
            }

            // Validate timestamp (prevent replay attacks)
            if (!DateTime.TryParse(timestamp, out DateTime requestTime) ||
                Math.Abs((DateTime.UtcNow - requestTime).TotalMinutes) > 2)
            {
                _logger.LogWarning("Invalid or expired timestamp from service '{ServiceName}'", serviceName);
                context.Result = new UnauthorizedResult();
                return;
            }

            // Validate signature
            String expectedSignature = GenerateSignature(serviceName, timestamp, _config.SharedSecret);
            if (!signature.Equals(expectedSignature, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Invalid signature from service '{ServiceName}'", serviceName);
                context.Result = new UnauthorizedResult();
                return;
            }

            _logger.LogDebug("Internal API call authorized for service '{ServiceName}'", serviceName);

            base.OnActionExecuting(context);
        }

        private String GetClientIP(HttpContext context)
        {
            return context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                   ?? context.Request.Headers["X-Real-IP"].FirstOrDefault()
                   ?? context.Connection.RemoteIpAddress?.ToString()
                   ?? "Unknown";
        }

        private String GenerateSignature(String serviceName, String timestamp, String secret)
        {
            String payload = $"{serviceName}:{timestamp}:{secret}";
            using HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return Convert.ToBase64String(hash);
        }
    }
}
