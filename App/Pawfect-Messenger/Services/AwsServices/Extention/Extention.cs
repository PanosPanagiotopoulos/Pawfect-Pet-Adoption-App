using Pawfect_Messenger.Data.Entities.Types.Aws;

namespace Pawfect_Messenger.Services.AwsServices.Extention
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
