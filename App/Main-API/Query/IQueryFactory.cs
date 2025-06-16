using Main_API.Query.Queries;

namespace Main_API.Query
{
    /// <summary>
    /// Interface for the query factory
    /// </summary>
    public interface IQueryFactory
    {
        T Query<T>() where T : IQuery;
    }
}
