using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Shelter;

namespace Pawfect_Pet_Adoption_App_API.Services.ShelterServices
{
	public interface IShelterService
	{
		Task<Shelter?> Persist(ShelterPersist persist, List<String> buildFields = null);
		Task Delete(String id);
		Task Delete(List<String> ids);
	}
}