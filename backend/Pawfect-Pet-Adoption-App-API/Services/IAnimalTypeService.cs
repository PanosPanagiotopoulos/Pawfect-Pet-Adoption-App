using Pawfect_Pet_Adoption_App_API.Models.AnimalType;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;

namespace Pawfect_Pet_Adoption_App_API.Services
{
    public interface IAnimalTypeService
    {
        // Συνάρτηση για query στα animal types
        Task<IEnumerable<AnimalTypeDto>> QueryAnimalTypesAsync(AnimalTypeLookup animalTypeLookup);
    }
}