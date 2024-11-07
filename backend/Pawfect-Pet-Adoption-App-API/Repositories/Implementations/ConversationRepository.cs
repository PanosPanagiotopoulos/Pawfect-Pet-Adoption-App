using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;
using Pawfect_Pet_Adoption_App_API.Services;

namespace Pawfect_Pet_Adoption_App_API.Repositories.Implementations
{
    public class ConversationRepository : GeneralRepo<Conversation>, IConversationRepository
    {
        public ConversationRepository(MongoDbService dbService) : base(dbService) { }
    }
}
