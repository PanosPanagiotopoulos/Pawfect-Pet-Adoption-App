using Pawfect_Messenger.Data.Entities;

namespace Pawfect_Messenger.Query.Interfaces
{
    /// <summary>
    /// Repository όπου διατηρούμε τις μη κοινές λειτουργίες
    /// Για το Collection: Conversation
    /// </summary>
    public interface IConversationRepository : IMongoRepository<Conversation>
    {
    }
}
