using MongoDB.Driver;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Query;

namespace Pawfect_Pet_Adoption_App_API.Services.FilterServices
{
    public class UserFilterBuilder: IFilterBuilder<User, UserLookup>
    {
        private readonly IQueryFactory _queryFactory;

        public UserFilterBuilder
        (
            IQueryFactory queryFactory
        )
        {
            this._queryFactory = queryFactory;
        }

        public async Task<FilterDefinition<User>> Build(UserLookup lookup)
        {
            FilterDefinitionBuilder<User> builder = Builders<User>.Filter;
            FilterDefinition<User> filter = builder.Empty;

            return await lookup.EnrichLookup(_queryFactory).ApplyFilters();
        }
    }
}
