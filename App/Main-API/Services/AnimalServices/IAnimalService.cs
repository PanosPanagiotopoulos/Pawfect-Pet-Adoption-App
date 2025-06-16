using Main_API.Models.Animal;
using Main_API.Models.Lookups;

namespace Main_API.Services.AnimalServices
{
	public interface IAnimalService
	{
		// Συνάρτηση για query στα animals
		Task<Animal?> Persist(AnimalPersist persist, List<String> fields);
		Task Delete(String id);
		Task Delete(List<String> ids);
	}
}
