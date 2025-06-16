using Main_API.Middleware;

namespace Pawfect_Pet_Adoption_App_API.Middlewares
{
    public class VerifiedUserMiddleware
    {
        private readonly RequestDelegate _next;

        public VerifiedUserMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only check for authenticated users
            if (context.User.Identity?.IsAuthenticated == true)
            {
                System.Security.Claims.Claim isVerifiedClaim = context.User.FindFirst("isVerified");
                if (isVerifiedClaim == null || !bool.TryParse(isVerifiedClaim.Value, out Boolean isVerified) || !isVerified)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("User is not verified.");
                    return;
                }
            }

            await _next(context);
        }
    }
    public static class JwtRevocationMiddlewareExtensions
    {
        public static IApplicationBuilder UseVerifiedUserMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<VerifiedUserMiddleware>();
        }
    }
}
