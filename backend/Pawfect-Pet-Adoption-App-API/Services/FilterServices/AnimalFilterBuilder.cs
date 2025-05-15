using MongoDB.Driver;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Query;

namespace Pawfect_Pet_Adoption_App_API.Services.FilterServices
{
    public class AnimalFilterBuilder : IFilterBuilder<Animal, AnimalLookup>
    {
        private readonly IQueryFactory _queryFactory;

        public AnimalFilterBuilder
        (
            IQueryFactory queryFactory
        )
        {
            this._queryFactory = queryFactory;
        }

        public async Task<FilterDefinition<Animal>> Build(AnimalLookup lookup)
        {
            FilterDefinitionBuilder<Animal> builder = Builders<Animal>.Filter;
            FilterDefinition<Animal> filter = builder.Empty;

            return await lookup.EnrichLookup(_queryFactory).ApplyFilters();
        }
    }
}
