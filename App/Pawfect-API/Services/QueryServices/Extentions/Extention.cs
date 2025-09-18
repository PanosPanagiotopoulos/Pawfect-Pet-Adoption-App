using Pawfect_API.Builders;
using Pawfect_API.Censors;
using Pawfect_API.Models.Lookups;
using Pawfect_API.Query;
using Pawfect_API.Query.Implementations;
using Pawfect_API.Query.Interfaces;
using Pawfect_API.Query.Queries;
using Pawfect_API.Repositories.Implementations;
using Pawfect_API.Repositories.Interfaces;
using Pawfect_API.Transactions;

namespace Pawfect_API.Services.QueryServices.Extentions
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
			services.AddScoped<IFileRepository, FileRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

            // Mongo Session Filter
            services.AddScoped<MongoTransactionFilter>();

            return services;
		}

	}
}
