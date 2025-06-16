using Main_API.Data.Entities;
using Main_API.Repositories.Interfaces;

namespace Main_API.Query.Interfaces
{
    /// <summary>
    /// Repository όπου διατηρούμε τις μη κοινές λειτουργίες
    /// Για το Collection: RefreshToken
    /// </summary>
    public interface IRefreshTokenRepository : IMongoRepository<RefreshToken>
    {
    }
}
