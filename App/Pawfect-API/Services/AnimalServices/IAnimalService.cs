using Pawfect_API.Models.Animal;
using Pawfect_API.Models.Lookups;

namespace Pawfect_API.Services.AnimalServices
{
	public interface IAnimalService
	{
		// Συνάρτηση για query στα animals
		Task<Animal?> Persist(AnimalPersist persist, List<String> fields);
		Task<List<Models.Animal.Animal>> PersistBatch(List<AnimalPersist> models, List<String> fields);
        Task Delete(String id);
		Task Delete(List<String> ids);
	}
}
