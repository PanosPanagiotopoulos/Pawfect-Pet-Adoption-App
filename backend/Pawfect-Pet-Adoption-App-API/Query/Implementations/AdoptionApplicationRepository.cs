using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;
using Pawfect_Pet_Adoption_App_API.Services;

namespace Pawfect_Pet_Adoption_App_API.Repositories.Implementations
{
    public class AdoptionApplicationRepository : GeneralRepo<AdoptionApplication>, IAdoptionApplicationRepository
    {
        public AdoptionApplicationRepository(MongoDbService dbService) : base(dbService) { }
    }
}
