using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Shelter;
using Pawfect_Pet_Adoption_App_API.Query.Queries;

namespace Pawfect_Pet_Adoption_App_API.Services.ShelterServices
{
	public class ShelterService : IShelterService
	{
		private readonly ShelterQuery _shelterQuery;
		private readonly ShelterBuilder _shelterBuilder;

		public ShelterService(ShelterQuery shelterQuery, ShelterBuilder shelterBuilder)
		{
			_shelterQuery = shelterQuery;
			_shelterBuilder = shelterBuilder;
		}

		public async Task<IEnumerable<ShelterDto>> QuerySheltersAsync(ShelterLookup shelterLookup)
		{
			List<Shelter> queriedShelters = await shelterLookup.EnrichLookup(_shelterQuery).CollectAsync();
			return await _shelterBuilder.SetLookup(shelterLookup).BuildDto(queriedShelters, shelterLookup.Fields.ToList());
		}
	}
}