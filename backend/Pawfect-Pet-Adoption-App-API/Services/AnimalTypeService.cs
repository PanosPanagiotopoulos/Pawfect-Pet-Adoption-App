using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.AnimalType;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Query.Queries;

namespace Pawfect_Pet_Adoption_App_API.Services
{
    public class AnimalTypeService : IAnimalTypeService
    {
        private readonly AnimalTypeQuery _animalTypeQuery;
        private readonly AnimalTypeBuilder _animalTypeBuilder;

        public AnimalTypeService(AnimalTypeQuery animalTypeQuery, AnimalTypeBuilder animalTypeBuilder)
        {
            _animalTypeQuery = animalTypeQuery;
            _animalTypeBuilder = animalTypeBuilder;
        }

        public async Task<IEnumerable<AnimalTypeDto>> QueryAnimalTypesAsync(AnimalTypeLookup animalTypeLookup)
        {
            List<AnimalType> queriedAnimalTypes = await animalTypeLookup.EnrichLookup(_animalTypeQuery).CollectAsync();
            return await _animalTypeBuilder.BuildDto(queriedAnimalTypes, animalTypeLookup.Fields.ToList());
        }
    }
}