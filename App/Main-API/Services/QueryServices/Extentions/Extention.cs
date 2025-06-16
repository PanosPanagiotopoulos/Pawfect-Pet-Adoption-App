using Main_API.Builders;
using Main_API.Censors;
using Main_API.Models.Lookups;
using Main_API.Query;
using Main_API.Query.Implementations;
using Main_API.Query.Interfaces;
using Main_API.Query.Queries;
using Main_API.Repositories.Implementations;
using Main_API.Repositories.Interfaces;
using Main_API.Transactions;

namespace Main_API.Services.QueryServices.Extentions
{
	public static class Extention
	{
		public static IServiceCollection AddQueryAndBuilderServices(this IServiceCollection services)
		{ 
			services.AddScoped<IQueryFactory, QueryFactory>();
            services.AddScoped<IBuilderFactory, BuilderFactory>();
            services.AddScoped<ICensorFactory, CensorFactory>();

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
				typeof(AutoAdoptionApplicationBuilder),
				typeof(AutoFileBuilder)
			);

			// Repositories
			services.AddScoped(typeof(IMongoRepository<>), typeof(BaseMongoRepository<>));
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
			services.AddScoped<IFileRepository, FileRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

            // Mongo Session Filter
            services.AddScoped<MongoTransactionFilter>();

            return services;
		}

	}
}
