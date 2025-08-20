using Pawfect_Notifications.Query.Queries;

namespace Pawfect_Notifications.Query
{
    /// <summary>
    /// Interface for the query factory
    /// </summary>
    public interface IQueryFactory
    {
        T Query<T>() where T : IQuery;
    }
}
