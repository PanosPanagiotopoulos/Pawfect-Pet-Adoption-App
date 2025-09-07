using MongoDB.Bson;
using MongoDB.Driver;
using Pawfect_Messenger.Models.Lookups;
using Pawfect_Messenger.Query;

namespace Pawfect_Messenger.Services.FilterServices
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
