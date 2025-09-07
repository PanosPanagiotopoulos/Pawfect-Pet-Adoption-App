using Pawfect_Messenger.Data.Entities;

namespace Pawfect_Messenger.Query.Interfaces
{
    /// <summary>
    /// Repository όπου διατηρούμε τις μη κοινές λειτουργίες
    /// Για το Collection: Message
    /// </summary>
    public interface IMessageRepository : IMongoRepository<Message>
    {
    }
}
