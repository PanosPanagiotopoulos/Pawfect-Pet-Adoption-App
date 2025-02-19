namespace Pawfect_Pet_Adoption_App_API.Services.ShelterServices.Extentions
{
	public static class Extention
	{
		public static IServiceCollection AddShelterServices(this IServiceCollection services)
		{
			services.AddScoped<IShelterService, ShelterService>();
			services.AddScoped(provider => new Lazy<IShelterService>(() => provider.GetRequiredService<IShelterService>()));


			return services;
		}
	}
}
