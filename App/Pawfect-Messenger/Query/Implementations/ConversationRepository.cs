using Pawfect_Messenger.Data.Entities;
using Pawfect_Messenger.Query.Interfaces;
using Pawfect_Messenger.Services.MongoServices;

namespace Pawfect_Messenger.Query.Implementations
{
	public class ConversationRepository : BaseMongoRepository<Conversation>, IConversationRepository
	{
		public ConversationRepository(MongoDbService dbService) : base(dbService) { }
	}
}
