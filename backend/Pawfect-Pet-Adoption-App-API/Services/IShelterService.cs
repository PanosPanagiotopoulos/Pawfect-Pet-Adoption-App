using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Shelter;

namespace Pawfect_Pet_Adoption_App_API.Services
{
    public interface IShelterService
    {
        // Συνάρτηση για query στα shelters
        Task<IEnumerable<ShelterDto>> QuerySheltersAsync(ShelterLookup shelterLookup);
    }
}