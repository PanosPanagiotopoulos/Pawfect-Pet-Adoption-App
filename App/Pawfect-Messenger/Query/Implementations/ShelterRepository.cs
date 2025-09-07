using Pawfect_Messenger.Data.Entities;
using Pawfect_Messenger.Query.Interfaces;
using Pawfect_Messenger.Services.MongoServices;


namespace Pawfect_Messenger.Query.Implementations
{
	public class ShelterRepository : BaseMongoRepository<Shelter>, IShelterRepository
	{
		public ShelterRepository(MongoDbService dbService) : base(dbService) { }
	}
}
