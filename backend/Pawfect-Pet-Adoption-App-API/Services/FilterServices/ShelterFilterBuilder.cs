using MongoDB.Driver;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Query;

namespace Pawfect_Pet_Adoption_App_API.Services.FilterServices
{
    public class ShelterFilterBuilder: IFilterBuilder<Shelter, ShelterLookup>
    {
        private readonly IQueryFactory _queryFactory;

        public ShelterFilterBuilder
        (
            IQueryFactory queryFactory
        )
        {
            this._queryFactory = queryFactory;
        }

        public async Task<FilterDefinition<Shelter>> Build(ShelterLookup lookup)
        {
            FilterDefinitionBuilder<Shelter> builder = Builders<Shelter>.Filter;
            FilterDefinition<Shelter> filter = builder.Empty;

            return await lookup.EnrichLookup(_queryFactory).ApplyFilters();
        }
    }
}
