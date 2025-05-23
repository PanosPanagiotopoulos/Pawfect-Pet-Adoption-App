﻿using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Censors;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Query;
using Pawfect_Pet_Adoption_App_API.Query.Implementations;
using Pawfect_Pet_Adoption_App_API.Query.Interfaces;
using Pawfect_Pet_Adoption_App_API.Query.Queries;
using Pawfect_Pet_Adoption_App_API.Repositories.Implementations;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;
using Pawfect_Pet_Adoption_App_API.Transactions;

namespace Pawfect_Pet_Adoption_App_API.Services.QueryServices.Extentions
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
			services.AddScoped<IFileRepository, FileRepository>();

            // Mongo Session Filter
            services.AddScoped<MongoTransactionFilter>();

            return services;
		}

	}
}
