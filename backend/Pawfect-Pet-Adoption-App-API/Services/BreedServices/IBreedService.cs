using Pawfect_Pet_Adoption_App_API.Models.Breed;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;

namespace Pawfect_Pet_Adoption_App_API.Services.BreedServices
{
	public interface IBreedService
	{
		Task<BreedDto?> Persist(BreedPersist persist, List<String> fields);
		Task Delete(String id);
		Task Delete(List<String> ids);

	}
}