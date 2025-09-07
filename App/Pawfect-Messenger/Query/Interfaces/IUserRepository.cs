using Pawfect_Messenger.Data.Entities;

namespace Pawfect_Messenger.Query.Interfaces
{
    /// <summary>
    /// Repository όπου διατηρούμε τις μη κοινές λειτουργίες
    /// Για το Collection: User
    /// </summary>
    public interface IUserRepository : IMongoRepository<User>
    {
        // TEST METHOD //
        Task<IEnumerable<User>> GetAllAsync();
    }
}
