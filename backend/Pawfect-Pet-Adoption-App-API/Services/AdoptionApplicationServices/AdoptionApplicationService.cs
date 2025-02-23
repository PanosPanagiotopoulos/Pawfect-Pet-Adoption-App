using AutoMapper;

using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.AdoptionApplication;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Query.Queries;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;

namespace Pawfect_Pet_Adoption_App_API.Services.AdoptionApplicationServices
{
	public class AdoptionApplicationService : IAdoptionApplicationService
	{
		private readonly AdoptionApplicationQuery _adoptionApplicationQuery;
		private readonly AdoptionApplicationBuilder _adoptionApplicationBuilder;
		private readonly IAdoptionApplicationRepository _adoptionApplicationRepository;
		private readonly IMapper _mapper;

		public AdoptionApplicationService(
			AdoptionApplicationQuery adoptionApplicationQuery,
			AdoptionApplicationBuilder adoptionApplicationBuilder,
			IAdoptionApplicationRepository adoptionApplicationRepository,
			IMapper mapper
			)
		{
			_adoptionApplicationQuery = adoptionApplicationQuery;
			_adoptionApplicationBuilder = adoptionApplicationBuilder;
			_adoptionApplicationRepository = adoptionApplicationRepository;
			_mapper = mapper;
		}
		public async Task<IEnumerable<AdoptionApplicationDto>> QueryAdoptionApplicationsAsync(AdoptionApplicationLookup adoptionApplicationLookup)
		{
			List<AdoptionApplication> queriedAdoptionApplications = await adoptionApplicationLookup.EnrichLookup(_adoptionApplicationQuery).CollectAsync();
			return await _adoptionApplicationBuilder.SetLookup(adoptionApplicationLookup).BuildDto(queriedAdoptionApplications, adoptionApplicationLookup.Fields.ToList());
		}

		public async Task<AdoptionApplicationDto?> Get(String id, List<String> fields)
		{
			//*TODO* Add authorization service with user roles and permissions

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

		public async Task<AdoptionApplicationDto?> Persist(AdoptionApplicationPersist persist)
		{
			Boolean isUpdate = await _adoptionApplicationRepository.ExistsAsync(x => x.Id == persist.Id);
			AdoptionApplication data = new AdoptionApplication();
			String dataId = String.Empty;
			if (isUpdate)
			{
				_mapper.Map(persist, data);
				data.UpdatedAt = DateTime.UtcNow;
				dataId = await _adoptionApplicationRepository.UpdateAsync(data);
			}
			else
			{
				_mapper.Map(persist, data);
				data.Id = null;
				data.CreatedAt = DateTime.UtcNow;
				data.UpdatedAt = DateTime.UtcNow;
				dataId = await _adoptionApplicationRepository.AddAsync(data);
			}

			if (String.IsNullOrEmpty(dataId))
			{
				throw new InvalidOperationException("Αποτυχία κατα το Persisting");
			}

			// Return dto model
			AdoptionApplicationLookup lookup = new AdoptionApplicationLookup(_adoptionApplicationQuery);
			lookup.Ids = new List<String> { dataId };
			lookup.Fields = new List<String> { "*", nameof(User) + ".*", nameof(Shelter) + ".*", nameof(Animal) + ".*" };
			lookup.Offset = 0;
			lookup.PageSize = 1;

			return (
					 await _adoptionApplicationBuilder.SetLookup(lookup)
					.BuildDto(await lookup.EnrichLookup().CollectAsync(), lookup.Fields.ToList())
					).FirstOrDefault();
		}
	}
}
