using MongoDB.Driver;

using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;
using Pawfect_Pet_Adoption_App_API.Services.MongoServices;
namespace Pawfect_Pet_Adoption_App_API.Repositories.Implementations
{
	public class UserRepository : BaseMongoRepository<User>, IUserRepository
	{
		public UserRepository(MongoDbService dbService) : base(dbService) { }

		public async Task<IEnumerable<User>> GetAllAsync()
		{
			return await _collection.Find(_ => true).ToListAsync();
		}

	}
}
