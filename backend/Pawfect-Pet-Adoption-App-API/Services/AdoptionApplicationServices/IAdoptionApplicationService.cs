using Pawfect_Pet_Adoption_App_API.Models.AdoptionApplication;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;

namespace Pawfect_Pet_Adoption_App_API.Services.AdoptionApplicationServices
{
	public interface IAdoptionApplicationService
	{
		// Συνάρτηση για query στα animals
		Task<AdoptionApplication?> Persist(AdoptionApplicationPersist persist, List<String> fields);
		Task Delete(String id);
		Task Delete(List<String> ids);
	}
}
