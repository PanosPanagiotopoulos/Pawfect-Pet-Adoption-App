using MongoDB.Driver;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Query;

namespace Pawfect_Pet_Adoption_App_API.Services.FilterServices
{
    public class MessageFilterBuilder: IFilterBuilder<Message, MessageLookup>
    {
        private readonly IQueryFactory _queryFactory;

        public MessageFilterBuilder
        (
            IQueryFactory queryFactory
        )
        {
            this._queryFactory = queryFactory;
        }

        public async Task<FilterDefinition<Message>> Build(MessageLookup lookup)
        {
            FilterDefinitionBuilder<Message> builder = Builders<Message>.Filter;
            FilterDefinition<Message> filter = builder.Empty;

            return await lookup.EnrichLookup(_queryFactory).ApplyFilters();
        }
    }
}
