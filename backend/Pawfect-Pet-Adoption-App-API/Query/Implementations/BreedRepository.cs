using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;
using Pawfect_Pet_Adoption_App_API.Services.MongoServices;

namespace Pawfect_Pet_Adoption_App_API.Repositories.Implementations
{
	public class BreedRepository : GeneralRepo<Breed>, IBreedRepository
	{
		public BreedRepository(MongoDbService dbService) : base(dbService) { }
	}
}
