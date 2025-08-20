namespace Pawfect_Notifications.Services.HttpServices.Extentions
{
	public static class Extention
	{
		public static IServiceCollection AddHttpServices(this IServiceCollection services)
		{
			services.AddSingleton<RequestService>();

			return services;
		}
	}
}
