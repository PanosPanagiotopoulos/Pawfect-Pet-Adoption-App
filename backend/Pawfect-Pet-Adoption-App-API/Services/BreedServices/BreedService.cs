using AutoMapper;

using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.Breed;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Query.Queries;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;

namespace Pawfect_Pet_Adoption_App_API.Services.BreedServices
{
	public class BreedService : IBreedService
	{
		private readonly BreedQuery _breedQuery;
		private readonly BreedBuilder _breedBuilder;
		private readonly IBreedRepository _breedRepository;
		private readonly IMapper _mapper;

		public BreedService
		(
			BreedQuery breedQuery,
			BreedBuilder breedBuilder,
			IBreedRepository breedRepository,
			IMapper mapper
		)
		{
			_breedQuery = breedQuery;
			_breedBuilder = breedBuilder;
			_breedRepository = breedRepository;
			_mapper = mapper;
		}

		public async Task<IEnumerable<BreedDto>> QueryBreedsAsync(BreedLookup breedLookup)
		{
			List<Breed> queriedBreeds = await breedLookup.EnrichLookup(_breedQuery).CollectAsync();
			return await _breedBuilder.SetLookup(breedLookup).BuildDto(queriedBreeds, breedLookup.Fields.ToList());
		}

		public async Task<BreedDto?> Persist(BreedPersist persist)
		{
			Boolean isUpdate = await _breedRepository.ExistsAsync(x => x.Id == persist.Id);
			Breed data = new Breed();
			String dataId = String.Empty;
			if (isUpdate)
			{
				_mapper.Map(persist, data);
				data.UpdatedAt = DateTime.UtcNow;
				dataId = await _breedRepository.UpdateAsync(data);
			}
			else
			{
				_mapper.Map(persist, data);
				data.Id = null; // Ensure new ID is generated
				data.CreatedAt = DateTime.UtcNow;
				data.UpdatedAt = DateTime.UtcNow;
				dataId = await _breedRepository.AddAsync(data);
			}

			if (String.IsNullOrEmpty(dataId))
			{
				throw new InvalidOperationException("Αποτυχία κατά το persist της φυλής");
			}

			// Return dto model
			BreedLookup lookup = new BreedLookup(_breedQuery);
			lookup.Ids = new List<String> { dataId };
			lookup.Fields = new List<String> { "*" };
			lookup.Offset = 0;
			lookup.PageSize = 1;

			return (
					 await _breedBuilder.SetLookup(lookup)
					.BuildDto(await lookup.EnrichLookup().CollectAsync(), lookup.Fields.ToList())
					).FirstOrDefault();
		}
	}
}