using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Apis;

namespace Pawfect_API.Services.NotificationServices.Extention
{
	public static class Extention
	{
		public static IServiceCollection AddNotificationServices(this IServiceCollection services, IConfiguration configuration)
		{
			services.Configure<NotificationApiConfig>(configuration);

			services.AddScoped<INotificationApiClient, NotificationApiClient>();
			services.AddScoped(provider => new Lazy<INotificationApiClient>(() => provider.GetRequiredService<INotificationApiClient>()));

			return services;
		}
	}
}
