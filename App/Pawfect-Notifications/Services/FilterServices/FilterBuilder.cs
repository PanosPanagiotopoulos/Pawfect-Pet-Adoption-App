using MongoDB.Bson;
using MongoDB.Driver;
using Pawfect_Notifications.Models.Lookups;
using Pawfect_Notifications.Query;

namespace Pawfect_Notifications.Services.FilterServices
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
