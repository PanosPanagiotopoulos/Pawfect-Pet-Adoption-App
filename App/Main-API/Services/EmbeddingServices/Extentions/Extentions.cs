namespace Pawfect_Pet_Adoption_App_API.Services.EmbeddingServices.Extentions
{
    public static class Extentions
    {
        public static IServiceCollection AddEmbeddingServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmbeddingConfig>(configuration);

            services.AddScoped<IEmbeddingService, EmbeddingService>();
            services.AddScoped(provider => new Lazy<IEmbeddingService>(() => provider.GetRequiredService<IEmbeddingService>()));

            return services;
        }
    }
}
