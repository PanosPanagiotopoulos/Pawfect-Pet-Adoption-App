using Main_API.Models.AdoptionApplication;
using Main_API.Models.Lookups;

namespace Main_API.Services.AdoptionApplicationServices
{
	public interface IAdoptionApplicationService
	{
		// Συνάρτηση για query στα animals
		Task<AdoptionApplication?> Persist(AdoptionApplicationPersist persist, List<String> fields);
		Task<Boolean> CanDeleteApplication(String applicationId);
		Task Delete(String id);
		Task Delete(List<String> ids);
	}
}
