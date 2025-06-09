using Pawfect_Pet_Adoption_App_API.Data.Entities;

namespace Pawfect_Pet_Adoption_App_API.Repositories.Interfaces
{
    /// <summary>
    /// Repository όπου διατηρούμε τις μη κοινές λειτουργίες
    /// Για το Collection: Message
    /// </summary>
    public interface IMessageRepository : IMongoRepository<Message>
    {
    }
}
