namespace Pawfect_Pet_Adoption_App_API.Services.AnimalTypeServices.Extentions
{
	public static class Extention
	{
		public static IServiceCollection AddAnimalTypeServices(this IServiceCollection services)
		{
			services.AddScoped<IAnimalTypeService, AnimalTypeService>();
			services.AddScoped(provider => new Lazy<IAnimalTypeService>(() => provider.GetRequiredService<IAnimalTypeService>()));

			return services;
		}
	}
}
