using Pawfect_Pet_Adoption_App_API.Query.Queries;

namespace Pawfect_Pet_Adoption_App_API.Query
{
    /// <summary>
    /// Interface for the query factory
    /// </summary>
    public interface IQueryFactory
    {
        T Query<T>() where T : class;
    }
}
