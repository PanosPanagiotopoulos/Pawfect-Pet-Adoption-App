using Pawfect_API.Data.Entities;
using Pawfect_API.Repositories.Interfaces;
using Pawfect_API.Services.MongoServices;

namespace Pawfect_API.Repositories.Implementations
{
	public class ConversationRepository : BaseMongoRepository<Conversation>, IConversationRepository
	{
		public ConversationRepository(MongoDbService dbService) : base(dbService) { }
	}
}
