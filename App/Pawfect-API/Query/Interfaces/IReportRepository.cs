using Pawfect_API.Data.Entities;

namespace Pawfect_API.Repositories.Interfaces
{
    /// <summary>
    /// Repository όπου διατηρούμε τις μη κοινές λειτουργίες
    /// Για το Collection: Report
    /// </summary>
    public interface IReportRepository : IMongoRepository<Report>
    {
    }
}
