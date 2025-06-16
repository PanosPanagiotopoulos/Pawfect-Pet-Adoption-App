using MongoDB.Driver;

using Main_API.Data.Entities;
using Main_API.Repositories.Interfaces;
using Main_API.Services.MongoServices;
namespace Main_API.Repositories.Implementations
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
