namespace Pawfect_Pet_Adoption_App_API.Services.BreedServices.Extention
{
	public static class Extention
	{
		public static IServiceCollection AddBreedServices(this IServiceCollection services)
		{
			services.AddScoped<IBreedService, BreedService>();
			services.AddScoped(provider => new Lazy<IBreedService>(() => provider.GetRequiredService<IBreedService>()));

			return services;
		}
	}
}
