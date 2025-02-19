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

		public async Task<AdoptionApplicationDto?> Get(String id, List<String> fields)
		{
			AdoptionApplicationLookup lookup = new AdoptionApplicationLookup(_adoptionApplicationQuery);
			lookup.Ids = new List<String> { id };
			lookup.Fields = fields;
			lookup.PageSize = 1;
			lookup.Offset = 0;

			List<AdoptionApplication> adoptionApplication = await lookup.EnrichLookup().CollectAsync();

			if (adoptionApplication == null)
			{
				throw new InvalidDataException("Δεν βρέθηκε αίτηση  υιοθεσίας με αυτό το ID");
			}

			return (await _adoptionApplicationBuilder.SetLookup(lookup).BuildDto(adoptionApplication, fields)).FirstOrDefault();
		}
	}
}
