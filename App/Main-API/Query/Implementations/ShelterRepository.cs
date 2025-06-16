using Main_API.Data.Entities;
using Main_API.Repositories.Interfaces;
using Main_API.Services.MongoServices;


namespace Main_API.Repositories.Implementations
{
	public class ShelterRepository : BaseMongoRepository<Shelter>, IShelterRepository
	{
		public ShelterRepository(MongoDbService dbService) : base(dbService) { }
	}
}
