using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;
using Pawfect_Pet_Adoption_App_API.Services;

namespace Pawfect_Pet_Adoption_App_API.Repositories.Implementations
{
    public class AnimalTypeRepository : GeneralRepo<AnimalType>, IAnimalTypeRepository
    {
        public AnimalTypeRepository(MongoDbService dbService) : base(dbService) { }
    }
}
