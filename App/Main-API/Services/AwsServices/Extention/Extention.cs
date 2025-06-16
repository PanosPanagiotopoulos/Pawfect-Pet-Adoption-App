using Main_API.Data.Entities.Types.Authentication;
using Main_API.Data.Entities.Types.Aws;
using Main_API.Services.AuthenticationServices;
using Main_API.Services.NotificationServices;

namespace Main_API.Services.AwsServices.Extention
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
