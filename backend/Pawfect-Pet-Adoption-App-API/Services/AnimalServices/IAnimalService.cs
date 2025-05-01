using Pawfect_Pet_Adoption_App_API.Models.Animal;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;

namespace Pawfect_Pet_Adoption_App_API.Services.AnimalServices
{
	public interface IAnimalService
	{
		// Συνάρτηση για query στα animals
		Task<IEnumerable<AnimalDto>> QueryAnimalsAsync(AnimalLookup animalLookup);

		Task<AnimalDto?> Get(String id, List<String> fields);
		Task<AnimalDto?> Persist(AnimalPersist persist, List<String> fields);
		Task Delete(String id);
		Task Delete(List<String> ids);
	}
}
