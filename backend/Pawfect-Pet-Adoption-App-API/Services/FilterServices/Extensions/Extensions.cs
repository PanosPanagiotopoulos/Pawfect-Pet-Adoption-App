using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Files;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Services.FileServices;

namespace Pawfect_Pet_Adoption_App_API.Services.FilterServices.Extensions
{
    public static class Extention
    {
        public static IServiceCollection AddFilterBuilderServices(this IServiceCollection services)
        {
            services.AddScoped<IFilterBuilder<User, UserLookup>, UserFilterBuilder>();
            services.AddScoped<IFilterBuilder<Shelter, ShelterLookup>, ShelterFilterBuilder>();
            services.AddScoped<IFilterBuilder<Report, ReportLookup>, ReportFilterBuilder>();
            services.AddScoped<IFilterBuilder<Conversation, ConversationLookup>, ConversationFilterBuilder>();
            services.AddScoped<IFilterBuilder<Message, MessageLookup>, MessageFilterBuilder>();
            services.AddScoped<IFilterBuilder<Animal, AnimalLookup>, AnimalFilterBuilder>();
            services.AddScoped<IFilterBuilder<AnimalType, AnimalTypeLookup>, AnimalTypeFilterBuilder>();
            services.AddScoped<IFilterBuilder<Breed, BreedLookup>, BreedFilterBuilder>();
            services.AddScoped<IFilterBuilder<AdoptionApplication, AdoptionApplicationLookup>, AdoptionApplicationFilterBuilder>();
            services.AddScoped<IFilterBuilder<Data.Entities.File, FileLookup>, FileFilterBuilder>();
            services.AddScoped<IFilterBuilder<Notification, NotificationLookup>, NotificationFilterBuilder>();

            return services;
        }
    }
}
