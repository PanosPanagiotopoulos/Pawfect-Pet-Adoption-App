using Pawfect_API.Data.Entities.Types.Apis;
using Pawfect_API.Data.Entities.Types.Authorisation;

namespace Pawfect_API.Services.AnimalServices.Extention
{
	public static class Extention
	{
		public static IServiceCollection AddAnimalServices(this IServiceCollection services, IConfiguration configuration)
		{
            services.Configure<AnimalsConfig>(configuration);

            services.AddScoped<IAnimalService, AnimalService>();
			services.AddScoped(provider => new Lazy<IAnimalService>(() => provider.GetRequiredService<IAnimalService>()));

			return services;
		}
	}
}
