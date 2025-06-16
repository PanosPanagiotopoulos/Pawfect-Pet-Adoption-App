using MongoDB.Bson;
using MongoDB.Driver;
using Main_API.Models.Lookups;
using Main_API.Query;

namespace Main_API.Services.FilterServices
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
