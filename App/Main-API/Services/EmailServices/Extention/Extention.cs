using Main_API.Data.Entities.Types.Apis;

namespace Main_API.Services.EmailServices.Extention
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
