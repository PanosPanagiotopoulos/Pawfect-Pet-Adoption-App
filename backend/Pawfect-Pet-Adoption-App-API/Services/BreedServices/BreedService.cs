using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.AdoptionApplication;
using Pawfect_Pet_Adoption_App_API.Models.Breed;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Query.Queries;
using Pawfect_Pet_Adoption_App_API.Repositories.Implementations;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;
using Pawfect_Pet_Adoption_App_API.Services.Convention;

namespace Pawfect_Pet_Adoption_App_API.Services.BreedServices
{
	public class BreedService : IBreedService
	{
		private readonly BreedQuery _breedQuery;
		private readonly BreedBuilder _breedBuilder;
		private readonly IBreedRepository _breedRepository;
		private readonly IMapper _mapper;
		private readonly IConventionService _conventionService;

		public BreedService
		(
			BreedQuery breedQuery,
			BreedBuilder breedBuilder,
			IBreedRepository breedRepository,
			IMapper mapper,
			IConventionService conventionService
		)
		{
			_breedQuery = breedQuery;
			_breedBuilder = breedBuilder;
			_breedRepository = breedRepository;
			_mapper = mapper;
			_conventionService = conventionService;
		}

		public async Task<IEnumerable<BreedDto>> QueryBreedsAsync(BreedLookup breedLookup)
		{
			List<Breed> queriedBreeds = await breedLookup.EnrichLookup(_breedQuery).CollectAsync();
			return await _breedBuilder.SetLookup(breedLookup).BuildDto(queriedBreeds, breedLookup.Fields.ToList());
		}

		public async Task<BreedDto?> Persist(BreedPersist persist, List<String> fields)
		{
			Boolean isUpdate = _conventionService.IsValidId(persist.Id);
			Breed data = new Breed();
			String dataId = String.Empty;
			if (isUpdate)
			{
				data = await _breedRepository.FindAsync(x => x.Id == persist.Id);

				if (data == null) throw new InvalidDataException("No entity found with id given");

				_mapper.Map(persist, data);
				data.UpdatedAt = DateTime.UtcNow;
			}
			else
			{
				_mapper.Map(persist, data);
				data.Id = null; // Ensure new ID is generated
				data.CreatedAt = DateTime.UtcNow;
				data.UpdatedAt = DateTime.UtcNow;
			}

			if (isUpdate) dataId = await _breedRepository.UpdateAsync(data);
			else dataId = await _breedRepository.AddAsync(data);

			if (String.IsNullOrEmpty(dataId))
			{
				throw new InvalidOperationException("Αποτυχία κατά το persist της φυλής");
			}

			// Return dto model
			BreedLookup lookup = new BreedLookup(_breedQuery);
			lookup.Ids = new List<String> { dataId };
			lookup.Fields = fields;
			lookup.Offset = 0;
			lookup.PageSize = 1;

			return (
					 await _breedBuilder.SetLookup(lookup)
					.BuildDto(await lookup.EnrichLookup().CollectAsync(), lookup.Fields.ToList())
					).FirstOrDefault();
		}

		public async Task Delete(String id) { await this.Delete(new List<String>() { id }); }

		public async Task Delete(List<String> ids)
		{
			// TODO : Authorization
			await _breedRepository.DeleteAsync(ids);
		}
	}
}