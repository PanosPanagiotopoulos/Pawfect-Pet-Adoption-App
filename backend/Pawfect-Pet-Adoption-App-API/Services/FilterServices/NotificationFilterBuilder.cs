using MongoDB.Driver;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Query;

namespace Pawfect_Pet_Adoption_App_API.Services.FilterServices
{
    public class NotificationFilterBuilder: IFilterBuilder<Notification, NotificationLookup>
    {
        private readonly IQueryFactory _queryFactory;

        public NotificationFilterBuilder
        (
            IQueryFactory queryFactory
        )
        {
            this._queryFactory = queryFactory;
        }

        public async Task<FilterDefinition<Notification>> Build(NotificationLookup lookup)
        {
            FilterDefinitionBuilder<Notification> builder = Builders<Notification>.Filter;
            FilterDefinition<Notification> filter = builder.Empty;

            return await lookup.EnrichLookup(_queryFactory).ApplyFilters();
        }
    }
}
