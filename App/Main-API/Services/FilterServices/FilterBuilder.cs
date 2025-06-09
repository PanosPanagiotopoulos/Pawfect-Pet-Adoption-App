using MongoDB.Bson;
using MongoDB.Driver;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Query;

namespace Pawfect_Pet_Adoption_App_API.Services.FilterServices
{
    public class FilterBuilder : IFilterBuilder
    {
        private readonly IQueryFactory _queryFactory;

        public FilterBuilder
        (
            IQueryFactory queryFactory    
        )
        {
            _queryFactory = queryFactory;
        }
        public async Task<FilterDefinition<BsonDocument>> Build(Lookup lookup)
        {
            return await lookup.ToFilters(_queryFactory);
        }
    }
}
