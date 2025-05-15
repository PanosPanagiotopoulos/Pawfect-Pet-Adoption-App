using MongoDB.Driver;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Query;

namespace Pawfect_Pet_Adoption_App_API.Services.FilterServices
{
    public class BreedFilterBuilder: IFilterBuilder<Breed, BreedLookup>
    {
        private readonly IQueryFactory _queryFactory;

        public BreedFilterBuilder
        (
            IQueryFactory queryFactory
        )
        {
            this._queryFactory = queryFactory;
        }

        public async Task<FilterDefinition<Breed>> Build(BreedLookup lookup)
        {
            FilterDefinitionBuilder<Breed> builder = Builders<Breed>.Filter;
            FilterDefinition<Breed> filter = builder.Empty;

            return await lookup.EnrichLookup(_queryFactory).ApplyFilters();
        }
    }
}

