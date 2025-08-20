namespace Pawfect_API.BackgroundTasks.RefreshTokensCleanupTask.Extensions
{
    public static class Extention
    {
        public static IServiceCollection AddRefreshTokenCleanupTask(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RefreshTokensCleanupTaskConfig>(configuration);

            services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService, RefreshTokensCleanupTask>();

            return services;
        }
    }

}
