using Pawfect_API.Models.AdoptionApplication;
using Pawfect_API.Models.Lookups;

namespace Pawfect_API.Services.AdoptionApplicationServices
{
	public interface IAdoptionApplicationService
	{
		// Συνάρτηση για query στα animals
		Task<AdoptionApplication?> Persist(AdoptionApplicationPersist persist, List<String> fields);
		Task<Boolean> CanDeleteApplication(String applicationId);
        Task<String> AdoptionRequestExists(String animalId);
        Task Delete(String id);
        Task DeleteFromAnimal(String id);
        Task DeleteFromAnimals(List<String> ids);
		Task Delete(List<String> ids);
	}
}
