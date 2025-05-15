using MongoDB.Driver;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Query;

namespace Pawfect_Pet_Adoption_App_API.Services.FilterServices
{
    public class ConversationFilterBuilder: IFilterBuilder<Conversation, ConversationLookup>
    {
        private readonly IQueryFactory _queryFactory;
        public ConversationFilterBuilder
        (
            IQueryFactory queryFactory
        )
        {
            this._queryFactory = queryFactory;
        }

        public async Task<FilterDefinition<Conversation>> Build(ConversationLookup lookup)
        {
            FilterDefinitionBuilder<Conversation> builder = Builders<Conversation>.Filter;
            FilterDefinition<Conversation> filter = builder.Empty;

            return await lookup.EnrichLookup(_queryFactory).ApplyFilters();
        }
    }
}
