using Pawfect_Pet_Adoption_App_API.Models.Breed;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;

namespace Pawfect_Pet_Adoption_App_API.Services.BreedServices
{
	public interface IBreedService
	{
		// Συνάρτηση για query στα breeds
		Task<IEnumerable<BreedDto>> QueryBreedsAsync(BreedLookup breedLookup);
	}
}