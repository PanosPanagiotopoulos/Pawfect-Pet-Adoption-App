using Main_API.Data.Entities;
using Main_API.Query.Interfaces;
using Main_API.Repositories.Implementations;
using Main_API.Repositories.Interfaces;
using Main_API.Services.MongoServices;

namespace Main_API.Query.Implementations
{
    public class RefreshTokenRepository : BaseMongoRepository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(MongoDbService dbService) : base(dbService) { }
    }
}
