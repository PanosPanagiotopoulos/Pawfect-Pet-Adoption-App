using AutoMapper;

using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.AdoptionApplication;
using Pawfect_Pet_Adoption_App_API.Models.AnimalType;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Query;
using Pawfect_Pet_Adoption_App_API.Query.Queries;
using Pawfect_Pet_Adoption_App_API.Repositories.Implementations;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;
using Pawfect_Pet_Adoption_App_API.Services.Convention;

namespace Pawfect_Pet_Adoption_App_API.Services.AnimalTypeServices
{
	public class AnimalTypeService : IAnimalTypeService
	{
		private readonly ILogger<AnimalTypeService> logger;
		private readonly IAnimalTypeRepository _animalTypeRepository;
		private readonly IConventionService _conventionService;
		private readonly IMapper _mapper;
        private readonly IQueryFactory _queryFactory;
        private readonly IBuilderFactory _builderFactory;
        private readonly ILogger<AnimalTypeService> _logger;
		public AnimalTypeService
		(
			ILogger<AnimalTypeService> logger,
			IAnimalTypeRepository animalTypeRepository,
			IConventionService conventionService,
			IMapper mapper,
			IQueryFactory queryFactory,
            IBuilderFactory builderFactory
        )
		{
			_logger = logger;
			_animalTypeRepository = animalTypeRepository;
			_conventionService = conventionService;
			_mapper = mapper;
            _queryFactory = queryFactory;
            _builderFactory = builderFactory;
        }

		public async Task<AnimalTypeDto?> Persist(AnimalTypePersist persist, List<String> fields)
		{
			Boolean isUpdate = _conventionService.IsValidId(persist.Id);
			AnimalType data = new AnimalType();
			String dataId = String.Empty;
			if (isUpdate)
			{
				data = await _animalTypeRepository.FindAsync(x => x.Id == persist.Id);

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

			if (isUpdate) dataId = await _animalTypeRepository.UpdateAsync(data);
			else dataId = await _animalTypeRepository.AddAsync(data);

			if (String.IsNullOrEmpty(dataId))
			{
				throw new InvalidOperationException("Αποτυχία κατά το persist του τύπου ζώου");
			}

			// Return dto model
			AnimalTypeLookup lookup = new AnimalTypeLookup();
			lookup.Ids = new List<String> { dataId };
			lookup.Fields = fields;
			lookup.Offset = 0;
			lookup.PageSize = 1;

			return (await _builderFactory.Builder<AnimalTypeBuilder>().BuildDto(await lookup.EnrichLookup(_queryFactory).CollectAsync(), fields)).FirstOrDefault();
		}

		public async Task Delete(String id) { await this.Delete(new List<String>() { id }); }

		public async Task Delete(List<String> ids)
		{
			// TODO : Authorization
			await _animalTypeRepository.DeleteAsync(ids);
		}
	}
}