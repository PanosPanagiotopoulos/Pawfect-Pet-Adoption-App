using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.Breed;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Query.Queries;

namespace Pawfect_Pet_Adoption_App_API.Services
{
    public class BreedService : IBreedService
    {
        private readonly BreedQuery _breedQuery;
        private readonly BreedBuilder _breedBuilder;

        public BreedService(BreedQuery breedQuery, BreedBuilder breedBuilder)
        {
            _breedQuery = breedQuery;
            _breedBuilder = breedBuilder;
        }

        public async Task<IEnumerable<BreedDto>> QueryBreedsAsync(BreedLookup breedLookup)
        {
            List<Breed> queriedBreeds = await breedLookup.EnrichLookup(_breedQuery).CollectAsync();
            return await _breedBuilder.BuildDto(queriedBreeds, breedLookup.Fields.ToList());
        }
    }
}