using Pawfect_Pet_Adoption_App_API.BackgroundTasks.UnverifiedUserCleanupTask;

namespace Pawfect_Pet_Adoption_App_API.BackgroundTasks.TemporaryFilesCleanupTask.Extensions
{
    public static class Extention
    {
        public static IServiceCollection AddTemporaryFilesCleanupTask(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<TemporaryFilesCleanupTaskConfig>(configuration);

            services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService, TemporaryFilesCleanupTask>();

            return services;
        }
    }
}
