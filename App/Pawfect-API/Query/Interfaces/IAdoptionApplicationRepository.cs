using Pawfect_API.Data.Entities;

namespace Pawfect_API.Repositories.Interfaces
{
    /// <summary>
    /// Repository όπου διατηρούμε τις μη κοινές λειτουργίες
    /// Για το Collection: AdoptionApplication
    /// </summary>
    public interface IAdoptionApplicationRepository : IMongoRepository<AdoptionApplication>
    {
    }
}
