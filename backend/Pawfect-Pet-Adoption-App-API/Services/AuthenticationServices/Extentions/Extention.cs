using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authentication;

namespace Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices.Extentions
{
	public static class Extention
	{
		public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
		{
			services.Configure<JwtConfig>(configuration.GetSection("Jwt"));
			services.Configure<GoogleOauth2Config>(configuration.GetSection("Google"));

			services.AddScoped<IAuthService, AuthService>();
			services.AddSingleton<JwtService>();

			return services;
		}
	}
}
