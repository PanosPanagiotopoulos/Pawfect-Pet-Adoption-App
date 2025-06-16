namespace Main_API.Services.AnimalServices.Extention
{
	public static class Extention
	{
		public static IServiceCollection AddAnimalServices(this IServiceCollection services)
		{
			services.AddScoped<IAnimalService, AnimalService>();
			services.AddScoped(provider => new Lazy<IAnimalService>(() => provider.GetRequiredService<IAnimalService>()));

			return services;
		}
	}
}
