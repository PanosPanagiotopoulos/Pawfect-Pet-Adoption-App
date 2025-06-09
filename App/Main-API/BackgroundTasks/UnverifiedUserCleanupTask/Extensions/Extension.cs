using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Censors;
using Pawfect_Pet_Adoption_App_API.Query.Implementations;
using Pawfect_Pet_Adoption_App_API.Query.Interfaces;
using Pawfect_Pet_Adoption_App_API.Query;
using Pawfect_Pet_Adoption_App_API.Repositories.Implementations;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;
using Pawfect_Pet_Adoption_App_API.Transactions;

namespace Pawfect_Pet_Adoption_App_API.BackgroundTasks.UnverifiedUserCleanupTask.Extensions
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
