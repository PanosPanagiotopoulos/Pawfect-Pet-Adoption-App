using AutoMapper;

using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.AnimalType;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Query.Queries;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;
using Pawfect_Pet_Adoption_App_API.Services.Convention;

namespace Pawfect_Pet_Adoption_App_API.Services.AnimalTypeServices
{
	public class AnimalTypeService : IAnimalTypeService
	{
		private readonly ILogger<AnimalTypeService> logger;
		private readonly AnimalTypeQuery _animalTypeQuery;
		private readonly AnimalTypeBuilder _animalTypeBuilder;
		private readonly IAnimalTypeRepository _animalTypeRepository;
		private readonly IConventionService _conventionService;
		private readonly IMapper _mapper;
		private readonly ILogger<AnimalTypeService> _logger;
		public AnimalTypeService
		(
			ILogger<AnimalTypeService> logger,
			AnimalTypeQuery animalTypeQuery,
			AnimalTypeBuilder animalTypeBuilder,
			IAnimalTypeRepository animalTypeRepository,
			IConventionService conventionService,
			IMapper mapper
		)
		{
			_logger = logger;
			_animalTypeQuery = animalTypeQuery;
			_animalTypeBuilder = animalTypeBuilder;
			_animalTypeRepository = animalTypeRepository;
			_conventionService = conventionService;
			_mapper = mapper;
		}

		public async Task<IEnumerable<AnimalTypeDto>> QueryAnimalTypesAsync(AnimalTypeLookup animalTypeLookup)
		{
			List<AnimalType> queriedAnimalTypes = await animalTypeLookup.EnrichLookup(_animalTypeQuery).CollectAsync();
			return await _animalTypeBuilder.SetLookup(animalTypeLookup).BuildDto(queriedAnimalTypes, animalTypeLookup.Fields.ToList());
		}

		public async Task<AnimalTypeDto?> Persist(AnimalTypePersist persist)
		{
			Boolean isUpdate = _conventionService.IsValidId(persist.Id);
			AnimalType data = new AnimalType();
			String dataId = String.Empty;
			if (isUpdate)
			{
				_mapper.Map(persist, data);
				data.UpdatedAt = DateTime.UtcNow;
				dataId = await _animalTypeRepository.UpdateAsync(data);
			}
			else
			{
				_mapper.Map(persist, data);
				data.Id = null; // Ensure new ID is generated
				data.CreatedAt = DateTime.UtcNow;
				data.UpdatedAt = DateTime.UtcNow;
				dataId = await _animalTypeRepository.AddAsync(data);
			}

			if (String.IsNullOrEmpty(dataId))
			{
				throw new InvalidOperationException("Αποτυχία κατά το persist του τύπου ζώου");
			}

			// Return dto model
			AnimalTypeLookup lookup = new AnimalTypeLookup(_animalTypeQuery);
			lookup.Ids = new List<String> { dataId };
			lookup.Fields = new List<String> { "*" };
			lookup.Offset = 0;
			lookup.PageSize = 1;

			return (
					 await _animalTypeBuilder.SetLookup(lookup)
					.BuildDto(await lookup.EnrichLookup().CollectAsync(), lookup.Fields.ToList())
					).FirstOrDefault();
		}
	}
}