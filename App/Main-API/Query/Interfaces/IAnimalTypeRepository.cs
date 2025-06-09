using Pawfect_Pet_Adoption_App_API.Data.Entities;

namespace Pawfect_Pet_Adoption_App_API.Repositories.Interfaces
{
    /// <summary>
    /// Repository όπου διατηρούμε τις μη κοινές λειτουργίες
    /// Για το Collection: AnimalType
    /// </summary>
    public interface IAnimalTypeRepository : IMongoRepository<AnimalType>
    {
    }
}
