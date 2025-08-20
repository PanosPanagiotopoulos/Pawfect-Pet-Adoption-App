using Pawfect_API.Data.Entities.Types.Authentication;
using Pawfect_API.Data.Entities.Types.Aws;
using Pawfect_API.Services.AuthenticationServices;
using Pawfect_API.Services.NotificationServices;

namespace Pawfect_API.Services.AwsServices.Extention
{
	public static class Extention
	{
		public static IServiceCollection AddAwsServices(this IServiceCollection services, IConfiguration configuration)
		{
			services.Configure<AwsConfig>(configuration);
			services.AddScoped<IAwsService, AwsService>();

			services.AddScoped(provider => new Lazy<IAwsService>(() => provider.GetRequiredService<IAwsService>()));


			return services;
		}
	}
}
