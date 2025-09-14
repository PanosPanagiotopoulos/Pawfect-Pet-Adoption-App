
namespace Pawfect_Messenger.Services.PresenceServices.Extentions
{
    public static class Extention
    {
        public static IServiceCollection AddPresenceServices(this IServiceCollection services)
        {
            services.AddScoped<IPresenceService, PresenceService>();

            return services;
        }
    }
}
