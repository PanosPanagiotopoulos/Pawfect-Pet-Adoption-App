using Main_API.Data.Entities;

namespace Main_API.Repositories.Interfaces
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
