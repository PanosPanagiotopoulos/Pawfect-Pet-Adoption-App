using Main_API.Data.Entities;

namespace Main_API.Repositories.Interfaces
{
    /// <summary>
    /// Repository όπου διατηρούμε τις μη κοινές λειτουργίες
    /// Για το Collection: Shelter
    /// </summary>
    public interface IShelterRepository : IMongoRepository<Shelter>
    {
    }
}
