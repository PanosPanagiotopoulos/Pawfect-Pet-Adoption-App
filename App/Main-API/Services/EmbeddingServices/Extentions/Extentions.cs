using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
using Pawfect_Pet_Adoption_App_API.Services.EmbeddingServices.AnimalEmbeddingValueExtractors;

namespace Pawfect_Pet_Adoption_App_API.Services.EmbeddingServices.Extentions
{
    public static class Extentions
    {
        public static IServiceCollection AddEmbeddingServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmbeddingConfig>(configuration);

            services.AddScoped<IEmbeddingService, EmbeddingService>();
            services.AddScoped(provider => new Lazy<IEmbeddingService>(() => provider.GetRequiredService<IEmbeddingService>()));

            services.AddTransient<EmbeddingValueExtractorFactory>();

            services.AddTransient<IAnimalEmbeddingValueExtractor, AnimalEmbeddingValueExtractor>();

            services.Configure<EmbeddingValueExtractorFactory.EmbeddingValueExtractorFactoryConfig>(x =>
            {
                // -- Animal --
                x.Add(EmbeddingValueExtractorType.Animal, typeof(IAnimalEmbeddingValueExtractor));
            });

            return services;
        }
    }
}
