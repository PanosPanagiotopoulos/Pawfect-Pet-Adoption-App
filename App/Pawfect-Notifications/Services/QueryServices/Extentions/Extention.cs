using Pawfect_Notifications.Builders;
using Pawfect_Notifications.Censors;
using Pawfect_Notifications.Query;
using Pawfect_Notifications.Repositories.Implementations;
using Pawfect_Notifications.Repositories.Interfaces;
using Pawfect_Notifications.Transactions;

namespace Pawfect_Notifications.Services.QueryServices.Extentions
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
				typeof(AutoNotificationBuilder)
			);

			// Repositories
			services.AddScoped(typeof(IMongoRepository<>), typeof(BaseMongoRepository<>));
			services.AddScoped<INotificationRepository, NotificationRepository>();

            // Mongo Session Filter
            services.AddScoped<MongoTransactionFilter>();

            return services;
		}

	}
}
