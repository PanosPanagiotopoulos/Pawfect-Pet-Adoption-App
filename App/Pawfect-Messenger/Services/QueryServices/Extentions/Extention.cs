using Pawfect_Messenger.Builders;
using Pawfect_Messenger.Censors;
using Pawfect_Messenger.Query.Implementations;
using Pawfect_Messenger.Query.Interfaces;
using Pawfect_Messenger.Query;
using Pawfect_Messenger.Transactions;

namespace Pawfect_Messenger.Services.QueryServices.Extentions
{
	public static class Extention
	{
		public static IServiceCollection AddQueryAndBuilderServices(this IServiceCollection services)
		{ 
			services.AddScoped<IQueryFactory, QueryFactory>();
            services.AddScoped<IBuilderFactory, BuilderFactory>();
            services.AddScoped<ICensorFactory, CensorFactory>();

			// Repositories
			services.AddScoped(typeof(IMongoRepository<>), typeof(BaseMongoRepository<>));
			services.AddScoped<IUserRepository, UserRepository>();
			services.AddScoped<IMessageRepository, MessageRepository>();
			services.AddScoped<IConversationRepository, ConversationRepository>();
            // Mongo Session Filter
            services.AddScoped<MongoTransactionFilter>();

            return services;
		}

	}
}
