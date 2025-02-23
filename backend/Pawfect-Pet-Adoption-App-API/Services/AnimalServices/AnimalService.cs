using AutoMapper;

using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.Animal;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Query.Queries;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;

namespace Pawfect_Pet_Adoption_App_API.Services.AnimalServices
{
	public class AnimalService : IAnimalService
	{
		private readonly AnimalQuery _animalQuery;
		private readonly AnimalBuilder _animalBuilder;
		private readonly IAnimalRepository _animalRepository;
		private readonly IMapper _mapper;

		public AnimalService
			(
				AnimalQuery animalQuery,
				AnimalBuilder animalBuilder,
				IAnimalRepository animalRepository,
				IMapper mapper
			)
		{
			_animalQuery = animalQuery;
			_animalBuilder = animalBuilder;
			_animalRepository = animalRepository;
			_mapper = mapper;
		}

		public async Task<IEnumerable<AnimalDto>> QueryAnimalsAsync(AnimalLookup animalLookup)
		{
			//*TODO* Add authorization service with user roles and permissions

			List<Animal> queriedAnimals = await animalLookup.EnrichLookup(_animalQuery).CollectAsync();
			return await _animalBuilder.SetLookup(animalLookup).BuildDto(queriedAnimals, animalLookup.Fields.ToList());
		}

		public async Task<AnimalDto?> Get(String id, List<String> fields)
		{
			//*TODO* Add authorization service with user roles and permissions

			AnimalLookup lookup = new AnimalLookup(_animalQuery);
			lookup.Ids = new List<String> { id };
			lookup.Fields = fields;
			lookup.PageSize = 1;
			lookup.Offset = 0;

			List<Animal> animal = await lookup.EnrichLookup().CollectAsync();

			if (animal == null)
			{
				throw new InvalidDataException("Δεν βρέθηκε ζώο με αυτό το ID");
			}

			return (await _animalBuilder.SetLookup(lookup).BuildDto(animal, fields)).FirstOrDefault();
		}

		public async Task<AnimalDto?> Persist(AnimalPersist persist)
		{
			Boolean isUpdate = await _animalRepository.ExistsAsync(x => x.Id == persist.Id);
			Animal data = new Animal();
			String dataId = String.Empty;

			//*TODO* Add authorization service with user roles and permissions

			//*TODO* Add AmazonS3 service integration for image upload/download or deletion

			if (isUpdate)
			{
				_mapper.Map(persist, data);
				data.UpdatedAt = DateTime.UtcNow;
				dataId = await _animalRepository.UpdateAsync(data);
			}
			else
			{
				_mapper.Map(persist, data);
				data.Id = null;
				data.CreatedAt = DateTime.UtcNow;
				data.UpdatedAt = DateTime.UtcNow;
				dataId = await _animalRepository.AddAsync(data);
			}

			if (String.IsNullOrEmpty(dataId))
			{
				throw new InvalidOperationException("Αποτυχία κατα το Persisting");
			}

			// Return dto model
			AnimalLookup lookup = new AnimalLookup(_animalQuery);
			lookup.Ids = new List<String> { dataId };
			lookup.Fields = new List<String> { "*", nameof(Shelter) + ".*", nameof(Breed) + ".*", nameof(AnimalType) + ".*" };
			lookup.Offset = 0;
			lookup.PageSize = 1;

			return (
					 await _animalBuilder.SetLookup(lookup)
					.BuildDto(await lookup.EnrichLookup().CollectAsync(), lookup.Fields.ToList())
					).FirstOrDefault();
		}
	}
}