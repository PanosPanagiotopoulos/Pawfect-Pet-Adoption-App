using Pawfect_API.Data.Entities.Types.AiAssistant;
using Pawfect_API.Services.AdoptionApplicationServices;

namespace Pawfect_API.Services.AiAssistantServices.Extentions
{
    public static class Extentions
    {
        public static IServiceCollection AddAiAssistantServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AiAssistantConfig>(configuration);

            services.AddScoped<IAiAssistantService, AiAssistantService>();
            services.AddScoped(provider => new Lazy<IAiAssistantService>(() => provider.GetRequiredService<IAiAssistantService>()));

            return services;
        }
    }
}
