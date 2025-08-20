namespace Pawfect_API.Services.NotificationServices.Extention
{
	public static class Extention
	{
		public static IServiceCollection AddNotificationServices(this IServiceCollection services)
		{
			services.AddScoped<INotificationApiClient, NotificationApiClient>();
			services.AddScoped(provider => new Lazy<INotificationApiClient>(() => provider.GetRequiredService<INotificationApiClient>()));

			return services;
		}
	}
}
