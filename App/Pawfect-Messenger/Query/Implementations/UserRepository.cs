using MongoDB.Driver;
using Pawfect_Messenger.Data.Entities;
using Pawfect_Messenger.Query.Interfaces;
using Pawfect_Messenger.Services.MongoServices;
namespace Pawfect_Messenger.Query.Implementations
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
