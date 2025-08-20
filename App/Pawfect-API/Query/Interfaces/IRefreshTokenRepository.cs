using Pawfect_API.Data.Entities;
using Pawfect_API.Repositories.Interfaces;

namespace Pawfect_API.Query.Interfaces
{
    /// <summary>
    /// Repository όπου διατηρούμε τις μη κοινές λειτουργίες
    /// Για το Collection: RefreshToken
    /// </summary>
    public interface IRefreshTokenRepository : IMongoRepository<RefreshToken>
    {
    }
}
