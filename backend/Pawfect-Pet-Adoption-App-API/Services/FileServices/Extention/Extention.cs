
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Apis;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Files;
using Pawfect_Pet_Adoption_App_API.Services.AdoptionApplicationServices;

namespace Pawfect_Pet_Adoption_App_API.Services.FileServices.Extention
{
	public static class Extention
	{
		public static IServiceCollection AddFileServices(this IServiceCollection services, IConfiguration configuration)
		{
			services.Configure<FilesConfig>(configuration);

			services.AddScoped<IFileService, FileService>();
			services.AddScoped(provider => new Lazy<IFileService>(() => provider.GetRequiredService<IFileService>()));

			return services;
		}
	}
}
