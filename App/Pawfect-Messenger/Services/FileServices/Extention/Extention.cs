using Pawfect_Messenger.Data.Entities.Types.Files;

namespace Pawfect_Messenger.Services.FileServices.Extention
{
	public static class Extention
	{
		public static IServiceCollection AddFileServices(this IServiceCollection services, IConfiguration configuration)
		{
			services.Configure<FilesConfig>(configuration);

            services.AddScoped<IFileAccessService, FileAccessService>();

            services.AddScoped(provider => new Lazy<IFileAccessService>(() => provider.GetRequiredService<IFileAccessService>()));

            return services;
		}
	}
}
