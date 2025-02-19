namespace Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices.Extentions
{
	public static class Extention
	{
		public static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
		{
			services.AddScoped<IAuthService, AuthService>();
			services.AddSingleton<JwtService>();

			return services;
		}
	}
}
