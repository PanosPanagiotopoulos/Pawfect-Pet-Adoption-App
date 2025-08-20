namespace Pawfect_API.Services.AdoptionApplicationServices.Extention
{
	public static class Extensions
	{
		public static IServiceCollection AddAdoptionApplicationServices(this IServiceCollection services)
		{
			services.AddScoped<IAdoptionApplicationService, AdoptionApplicationService>();
			services.AddScoped(provider => new Lazy<IAdoptionApplicationService>(() => provider.GetRequiredService<IAdoptionApplicationService>()));

			return services;
		}
	}
}
