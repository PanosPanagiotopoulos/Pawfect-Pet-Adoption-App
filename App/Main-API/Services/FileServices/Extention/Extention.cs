﻿
using Main_API.Data.Entities.Types.Apis;
using Main_API.Data.Entities.Types.Files;
using Main_API.Services.AdoptionApplicationServices;
using Pawfect_Pet_Adoption_App_API.Services.FileServices;

namespace Main_API.Services.FileServices.Extention
{
	public static class Extention
	{
		public static IServiceCollection AddFileServices(this IServiceCollection services, IConfiguration configuration)
		{
			services.Configure<FilesConfig>(configuration);

			services.AddScoped<IFileService, FileService>();
            services.AddScoped<IFileDataExtractor, ExcelExtractor>();

            services.AddScoped(provider => new Lazy<IFileService>(() => provider.GetRequiredService<IFileService>()));
            services.AddScoped(provider => new Lazy<IFileDataExtractor>(() => provider.GetRequiredService<IFileDataExtractor>()));

            return services;
		}
	}
}
