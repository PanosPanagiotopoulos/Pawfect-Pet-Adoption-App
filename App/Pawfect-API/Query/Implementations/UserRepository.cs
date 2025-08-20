using MongoDB.Driver;

using Pawfect_API.Data.Entities;
using Pawfect_API.Repositories.Interfaces;
using Pawfect_API.Services.MongoServices;
namespace Pawfect_API.Repositories.Implementations
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
