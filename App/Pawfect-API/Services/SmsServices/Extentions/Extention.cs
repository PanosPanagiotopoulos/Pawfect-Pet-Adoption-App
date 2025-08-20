using Pawfect_API.Data.Entities.Types.Apis;

namespace Pawfect_API.Services.SmsServices.Extentions
{
	public static class Extention
	{
		public static IServiceCollection AddSmsServices(this IServiceCollection services, IConfiguration configuration)
		{
			services.Configure<SmsApiConfig>(configuration);

			services.AddScoped<ISmsService, SmsService>();

			return services;
		}
	}
}
