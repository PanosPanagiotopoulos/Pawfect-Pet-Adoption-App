using Pawfect_API.Models.Lookups;
using Pawfect_API.Models.Shelter;

namespace Pawfect_API.Services.ShelterServices
{
	public interface IShelterService
	{
		Task<Shelter?> Persist(ShelterPersist persist, List<String> buildFields = null);
		Task Delete(String id);
		Task Delete(List<String> ids);
	}
}