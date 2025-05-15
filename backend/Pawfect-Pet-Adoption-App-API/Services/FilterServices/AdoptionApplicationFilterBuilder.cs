using MongoDB.Bson;
using MongoDB.Driver;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Query;
using Pawfect_Pet_Adoption_App_API.Query.Queries;

namespace Pawfect_Pet_Adoption_App_API.Services.FilterServices
{
    public class AdoptionApplicationFilterBuilder : IFilterBuilder<AdoptionApplication, AdoptionApplicationLookup>
    {
        private readonly IQueryFactory _queryFactory;

        public AdoptionApplicationFilterBuilder
        (
            IQueryFactory queryFactory
        )
        {
            this._queryFactory = queryFactory;
        }

        public async Task<FilterDefinition<AdoptionApplication>> Build(AdoptionApplicationLookup lookup)
        {
            FilterDefinitionBuilder<AdoptionApplication> builder = Builders<AdoptionApplication>.Filter;
            FilterDefinition<AdoptionApplication> filter = builder.Empty;

            return await lookup.EnrichLookup(_queryFactory).ApplyFilters();
        }
    }
}
