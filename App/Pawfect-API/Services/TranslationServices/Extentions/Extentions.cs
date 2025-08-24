using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Translation;

namespace Pawfect_Pet_Adoption_App_API.Services.TranslationServices.Extentions
{
    public static class Extention
    {
        public static IServiceCollection AddTranslationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<TranslationConfig>(configuration);

            services.AddScoped<ITranslationService, TranslationService>();

            return services;
        }
    }
}
