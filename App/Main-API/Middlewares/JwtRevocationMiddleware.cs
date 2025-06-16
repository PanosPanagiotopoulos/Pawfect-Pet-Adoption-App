using Main_API.Services.AuthenticationServices;

using System.IdentityModel.Tokens.Jwt;

namespace Main_API.Middleware
{
	/// <summary>
	/// Middleware to check if a JWT token has been revoked.
	/// </summary>
	public class JwtRevocationMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<JwtRevocationMiddleware> _logger;
		private readonly JwtService _jwtService;

		public JwtRevocationMiddleware(RequestDelegate next, ILogger<JwtRevocationMiddleware> logger, JwtService jwtService)
		{
			_next = next;
			_logger = logger;
			_jwtService = jwtService;
		}

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
            {
                String? token = context.Request.Cookies[JwtService.ACCESS_TOKEN];
                if (!String.IsNullOrEmpty(token))
                {
                    JwtSecurityToken jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
                    String? tokenId = jwtToken.Id;
                    if (!String.IsNullOrEmpty(tokenId))
                    {
                        if (_jwtService.IsTokenRevoked(tokenId))
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            await context.Response.WriteAsync("Forbidden: Token has been revoked.");
                            return;
                        }
                    }
                }
            }

            await _next(context);
        }
    }

	/// <summary>
	/// Extension method to add the JwtRevocationMiddleware to the application pipeline.
	/// </summary>
	public static class JwtRevocationMiddlewareExtensions
	{
		public static IApplicationBuilder UseJwtRevocation(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<JwtRevocationMiddleware>();
		}
	}
}