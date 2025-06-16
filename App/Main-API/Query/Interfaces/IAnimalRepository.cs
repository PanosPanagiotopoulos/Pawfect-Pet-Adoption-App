using Main_API.Data.Entities;

namespace Main_API.Repositories.Interfaces
{
    /// <summary>
    /// Repository όπου διατηρούμε τις μη κοινές λειτουργίες
    /// Για το Collection: Animal
    /// </summary>
    public interface IAnimalRepository : IMongoRepository<Animal>
    {
    }
}
