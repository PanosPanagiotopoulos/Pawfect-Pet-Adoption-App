using Pawfect_API.Builders;
using Pawfect_API.Censors;
using Pawfect_API.Query.Implementations;
using Pawfect_API.Query.Interfaces;
using Pawfect_API.Query;
using Pawfect_API.Repositories.Implementations;
using Pawfect_API.Repositories.Interfaces;
using Pawfect_API.Transactions;

namespace Pawfect_API.BackgroundTasks.UnverifiedUserCleanupTask.Extensions
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
