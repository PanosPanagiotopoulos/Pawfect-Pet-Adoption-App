namespace Pawfect_Pet_Adoption_App_API.Services.EmailServices.Extention
{
	public static class Extention
	{
		public static IServiceCollection AddEmailServices(this IServiceCollection services)
		{
			services.AddSingleton<IEmailService, EmailService>();

			return services;
		}
	}
}
