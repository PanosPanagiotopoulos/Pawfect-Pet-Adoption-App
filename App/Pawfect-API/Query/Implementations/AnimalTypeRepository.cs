using Pawfect_API.Data.Entities;
using Pawfect_API.Repositories.Interfaces;
using Pawfect_API.Services.MongoServices;

namespace Pawfect_API.Repositories.Implementations
{
	public class AnimalTypeRepository : BaseMongoRepository<AnimalType>, IAnimalTypeRepository
	{
		public AnimalTypeRepository(MongoDbService dbService) : base(dbService) { }
	}
}
