using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Query.Queries;
using Pawfect_Pet_Adoption_App_API.Repositories.Implementations;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;

namespace Pawfect_Pet_Adoption_App_API.Services.QueryServices.Extentions
{
	public static class Extention
	{
		public static IServiceCollection AddQueryAndBuilderServices(this IServiceCollection services)
		{
			services.AddScoped<UserQuery>();
			services.AddScoped<ShelterQuery>();
			services.AddScoped<ReportQuery>();
			services.AddScoped<NotificationQuery>();
			services.AddScoped<MessageQuery>();
			services.AddScoped<ConversationQuery>();
			services.AddScoped<BreedQuery>();
			services.AddScoped<AnimalQuery>();
			services.AddScoped<AnimalTypeQuery>();
			services.AddScoped<AdoptionApplicationQuery>();

			services.AddScoped<AnimalLookup>();
			services.AddScoped<UserLookup>();
			services.AddScoped<ShelterLookup>();
			services.AddScoped<NotificationLookup>();
			services.AddScoped<ReportLookup>();
			services.AddScoped<ConversationLookup>();
			services.AddScoped<MessageLookup>();
			services.AddScoped<AdoptionApplicationLookup>();
			services.AddScoped<BreedLookup>();
			services.AddScoped<AnimalTypeLookup>();

			services.AddScoped<AnimalBuilder>();
			services.AddScoped<UserBuilder>();
			services.AddScoped<ShelterBuilder>();
			services.AddScoped<NotificationBuilder>();
			services.AddScoped<ReportBuilder>();
			services.AddScoped<ConversationBuilder>();
			services.AddScoped<MessageBuilder>();
			services.AddScoped<AdoptionApplicationBuilder>();
			services.AddScoped<BreedBuilder>();
			services.AddScoped<AnimalTypeBuilder>();

			services.AddAutoMapper(
				typeof(AutoUserBuilder),
				typeof(AutoShelterBuilder),
				typeof(AutoReportBuilder),
				typeof(AutoNotificationBuilder),
				typeof(AutoMessageBuilder),
				typeof(AutoConversationBuilder),
				typeof(AutoAnimalTypeBuilder),
				typeof(AutoBreedBuilder),
				typeof(AutoAnimalBuilder),
				typeof(AutoAdoptionApplicationBuilder)
			);

			// Repositories
			services.AddScoped(typeof(IGeneralRepo<>), typeof(GeneralRepo<>));
			services.AddScoped<IUserRepository, UserRepository>();
			services.AddScoped<IShelterRepository, ShelterRepository>();
			services.AddScoped<IAnimalRepository, AnimalRepository>();
			services.AddScoped<IAnimalTypeRepository, AnimalTypeRepository>();
			services.AddScoped<IBreedRepository, BreedRepository>();
			services.AddScoped<IAdoptionApplicationRepository, AdoptionApplicationRepository>();
			services.AddScoped<IReportRepository, ReportRepository>();
			services.AddScoped<IMessageRepository, MessageRepository>();
			services.AddScoped<IConversationRepository, ConversationRepository>();
			services.AddScoped<INotificationRepository, NotificationRepository>();


			return services;
		}

	}
}
