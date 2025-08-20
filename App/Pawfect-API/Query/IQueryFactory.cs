using Pawfect_API.Query.Queries;

namespace Pawfect_API.Query
{
    /// <summary>
    /// Interface for the query factory
    /// </summary>
    public interface IQueryFactory
    {
        T Query<T>() where T : IQuery;
    }
}
