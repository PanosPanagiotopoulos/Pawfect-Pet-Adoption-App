using Pawfect_Notifications.BackgroundTasks.NotificationProcessor;

namespace Pawfect_Notifications.BackgroundTasks.Extentions
{
    public static class Extention
    {
        public static IServiceCollection AddBackgroundTasks(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<NotificationProcessorConfig>(configuration);

            services.AddHostedService<NotificationProcessorTask>();

            return services;
        }
    }
}
