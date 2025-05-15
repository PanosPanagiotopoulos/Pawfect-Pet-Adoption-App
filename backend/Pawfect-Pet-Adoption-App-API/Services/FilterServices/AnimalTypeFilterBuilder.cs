using MongoDB.Driver;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Query;

namespace Pawfect_Pet_Adoption_App_API.Services.FilterServices
{
    public class AnimalTypeFilterBuilder : IFilterBuilder<AnimalType, AnimalTypeLookup>
    {
        private readonly IQueryFactory _queryFactory;

        public AnimalTypeFilterBuilder
        (
            IQueryFactory queryFactory
        )
        {
            this._queryFactory = queryFactory;
        }

        public async Task<FilterDefinition<AnimalType>> Build(AnimalTypeLookup lookup)
        {
            FilterDefinitionBuilder<AnimalType> builder = Builders<AnimalType>.Filter;
            FilterDefinition<AnimalType> filter = builder.Empty;

            return await lookup.EnrichLookup(_queryFactory).ApplyFilters();
        }
    }
}
