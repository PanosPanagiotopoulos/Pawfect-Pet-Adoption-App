using Pawfect_Messenger.Query.Queries;

namespace Pawfect_Messenger.Query
{
    /// <summary>
    /// Interface for the query factory
    /// </summary>
    public interface IQueryFactory
    {
        T Query<T>() where T : IQuery;
    }
}
