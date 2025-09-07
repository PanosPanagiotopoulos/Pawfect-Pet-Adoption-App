namespace Pawfect_Messenger.Services.FilterServices.Extensions
{
    public static class Extention
    {
        public static IServiceCollection AddFilterBuilderServices(this IServiceCollection services)
        {
            services.AddScoped<IFilterBuilder, FilterBuilder>();

            return services;
        }
    }
}
