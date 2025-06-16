using Main_API.Builders;
using Main_API.Censors;
using Main_API.Query.Implementations;
using Main_API.Query.Interfaces;
using Main_API.Query;
using Main_API.Repositories.Implementations;
using Main_API.Repositories.Interfaces;
using Main_API.Transactions;

namespace Main_API.BackgroundTasks.UnverifiedUserCleanupTask.Extensions
{
    public static class Extention
    {
        public static IServiceCollection AddUnverifiedUserCleanupTask(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<UnverifiedUserCleanupTaskConfig>(configuration);

            services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService, UnverifiedUserCleanupTask>();

            return services;
        }

    }
}
