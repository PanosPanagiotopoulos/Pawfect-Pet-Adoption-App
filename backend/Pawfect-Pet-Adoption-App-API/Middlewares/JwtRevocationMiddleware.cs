using Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices;

using System.IdentityModel.Tokens.Jwt;

namespace Pawfect_Pet_Adoption_App_API.Middleware
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
			// Check if the user is authenticated
			if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
			{
				// Extract the token from the Authorization header
				String? authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

				if (!String.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
				{
					String token = authHeader.Substring("Bearer ".Length).Trim();

					try
					{
						// Read the JWT token
						JwtSecurityToken jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);

						// Extract the JTI (JWT ID) claim
						String? tokenId = jwtToken.Id;

						if (!String.IsNullOrEmpty(tokenId))
						{
							// Check if the token is revoked
							if (_jwtService.IsTokenRevoked(tokenId))
							{
								_logger.LogWarning("Revoked token detected. TokenId: {TokenId}", tokenId);
								context.Response.StatusCode = StatusCodes.Status403Forbidden;
								await context.Response.WriteAsync("Forbidden: Token has been revoked.");
								return;
							}
						}
						else
						{
							_logger.LogWarning("JWT token does not contain a JTI claim.");
						}
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "Error processing JWT token in revocation middleware.");
						context.Response.StatusCode = StatusCodes.Status400BadRequest;
						await context.Response.WriteAsync("Bad Request: Invalid JWT token.");
						return;
					}
				}
			}

			// Call the next middleware in the pipeline
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