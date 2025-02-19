using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.AdoptionApplication;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Query.Queries;

namespace Pawfect_Pet_Adoption_App_API.Services.AdoptionApplicationServices
{
	public class AdoptionApplicationService : IAdoptionApplicationService
	{
		private readonly AdoptionApplicationQuery _adoptionApplicationQuery;
		private readonly AdoptionApplicationBuilder _adoptionApplicationBuilder;

		public AdoptionApplicationService(AdoptionApplicationQuery adoptionApplicationQuery, AdoptionApplicationBuilder adoptionApplicationBuilder)
		{
			_adoptionApplicationQuery = adoptionApplicationQuery;
			_adoptionApplicationBuilder = adoptionApplicationBuilder;
		}
		public async Task<IEnumerable<AdoptionApplicationDto>> QueryAdoptionApplicationsAsync(AdoptionApplicationLookup adoptionApplicationLookup)
		{
			List<AdoptionApplication> queriedAdoptionApplications = await adoptionApplicationLookup.EnrichLookup(_adoptionApplicationQuery).CollectAsync();
			return await _adoptionApplicationBuilder.SetLookup(adoptionApplicationLookup).BuildDto(queriedAdoptionApplications, adoptionApplicationLookup.Fields.ToList());
		}
	}
}
