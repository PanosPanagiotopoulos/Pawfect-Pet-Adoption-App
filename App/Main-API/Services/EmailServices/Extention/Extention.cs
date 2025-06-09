using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Apis;

namespace Pawfect_Pet_Adoption_App_API.Services.EmailServices.Extention
{
	public static class Extention
	{
		public static IServiceCollection AddEmailServices(this IServiceCollection services, IConfiguration configuration)
		{
			services.Configure<EmailApiConfig>(configuration);

			services.AddSingleton<IEmailService, EmailService>();

			return services;
		}
	}
}
