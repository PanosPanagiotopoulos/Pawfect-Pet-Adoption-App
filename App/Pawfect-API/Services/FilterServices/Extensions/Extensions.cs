using Pawfect_API.Data.Entities;
using Pawfect_API.Data.Entities.Types.Files;
using Pawfect_API.Models.Lookups;
using Pawfect_API.Services.FileServices;

namespace Pawfect_API.Services.FilterServices.Extensions
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
