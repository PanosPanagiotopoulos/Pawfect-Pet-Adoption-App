using Main_API.Models.Breed;
using Main_API.Models.Lookups;

namespace Main_API.Services.BreedServices
{
	public interface IBreedService
	{
		Task<Breed?> Persist(BreedPersist persist, List<String> fields);
		Task Delete(String id);
		Task Delete(List<String> ids);

	}
}