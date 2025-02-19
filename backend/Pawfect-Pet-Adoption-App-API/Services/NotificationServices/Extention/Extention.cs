namespace Pawfect_Pet_Adoption_App_API.Services.NotificationServices.Extention
{
	public static class Extention
	{
		public static IServiceCollection AddNotificationServices(this IServiceCollection services)
		{
			services.AddScoped<INotificationService, NotificationService>();
			services.AddScoped(provider => new Lazy<INotificationService>(() => provider.GetRequiredService<INotificationService>()));

			return services;
		}
	}
}
