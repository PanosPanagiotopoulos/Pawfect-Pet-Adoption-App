using Pawfect_API.Data.Entities;
using Pawfect_API.Query.Interfaces;
using Pawfect_API.Repositories.Implementations;
using Pawfect_API.Repositories.Interfaces;
using Pawfect_API.Services.MongoServices;

namespace Pawfect_API.Query.Implementations
{
    public class RefreshTokenRepository : BaseMongoRepository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(MongoDbService dbService) : base(dbService) { }
    }
}
