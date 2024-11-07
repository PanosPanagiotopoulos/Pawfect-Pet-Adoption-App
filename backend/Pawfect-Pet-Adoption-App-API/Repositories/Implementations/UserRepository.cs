using MongoDB.Driver;
using Pawfect_Pet_Adoption_App_API.Models;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;
using Pawfect_Pet_Adoption_App_API.Services;

namespace Pawfect_Pet_Adoption_App_API.Repositories.Implementations
{
    public class UserRepository : GeneralRepo<User>, IUserRepository
    {
        public UserRepository(MongoDbService dbService) : base(dbService) { }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

    }
}
