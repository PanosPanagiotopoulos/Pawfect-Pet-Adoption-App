using Main_API.Data.Entities;

namespace Main_API.Repositories.Interfaces
{
    /// <summary>
    /// Repository όπου διατηρούμε τις μη κοινές λειτουργίες
    /// Για το Collection: AdoptionApplication
    /// </summary>
    public interface IAdoptionApplicationRepository : IMongoRepository<AdoptionApplication>
    {
    }
}
