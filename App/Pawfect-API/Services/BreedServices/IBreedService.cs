using Pawfect_API.Models.Breed;
using Pawfect_API.Models.Lookups;

namespace Pawfect_API.Services.BreedServices
{
	public interface IBreedService
	{
		Task<Breed?> Persist(BreedPersist persist, List<String> fields);
		Task Delete(String id);
		Task Delete(List<String> ids);

	}
}