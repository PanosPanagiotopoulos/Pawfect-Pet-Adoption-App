using Main_API.Data.Entities;
using Main_API.Data.Entities.Types.Files;
using Main_API.Models.Lookups;
using Main_API.Services.FileServices;

namespace Main_API.Services.FilterServices.Extensions
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
