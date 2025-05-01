using Pawfect_Pet_Adoption_App_API.Models.AnimalType;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;

namespace Pawfect_Pet_Adoption_App_API.Services.AnimalTypeServices
{
	public interface IAnimalTypeService
	{
		// Συνάρτηση για query στα animal types
		Task<IEnumerable<AnimalTypeDto>> QueryAnimalTypesAsync(AnimalTypeLookup animalTypeLookup);

		Task<AnimalTypeDto?> Persist(AnimalTypePersist persist, List<String> fields);

		Task Delete(String id);
		Task Delete(List<String> ids);

	}
}