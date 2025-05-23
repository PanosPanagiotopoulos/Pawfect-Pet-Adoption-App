﻿using AutoMapper;

using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Censors;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
using Pawfect_Pet_Adoption_App_API.Exceptions;
using Pawfect_Pet_Adoption_App_API.Models.AnimalType;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Query;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;
using Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices;
using Pawfect_Pet_Adoption_App_API.Services.BreedServices;
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
        private readonly IAuthorisationService _authorisationService;
        private readonly AuthContextBuilder _contextBuilder;
        private readonly Lazy<IBreedService> _breedService;
        private readonly ICensorFactory _censorFactory;
        public AnimalTypeService
		(
			ILogger<AnimalTypeService> logger,
			IAnimalTypeRepository animalTypeRepository,
			IConventionService conventionService,
			IMapper mapper,
			IQueryFactory queryFactory,
            IBuilderFactory builderFactory,
            IAuthorisationService authorisationService,
            AuthContextBuilder contextBuilder,
			Lazy<IBreedService> breedService,
            ICensorFactory censorFactory
        )
		{
			_logger = logger;
			_animalTypeRepository = animalTypeRepository;
			_conventionService = conventionService;
			_mapper = mapper;
            _queryFactory = queryFactory;
            _builderFactory = builderFactory;
            _authorisationService = authorisationService;
            _contextBuilder = contextBuilder;
            _breedService = breedService;
            _censorFactory = censorFactory;
        }

		public async Task<Models.AnimalType.AnimalType?> Persist(AnimalTypePersist persist, List<String> fields)
		{
            if (!await _authorisationService.AuthorizeAsync(Permission.EditAnimalTypes))
                throw new ForbiddenException("You are not authorized to edit animal types", typeof(Data.Entities.AnimalType), Permission.EditAnimalTypes);

            Boolean isUpdate = _conventionService.IsValidId(persist.Id);
            Data.Entities.AnimalType data = new Data.Entities.AnimalType();
			String dataId = String.Empty;
			if (isUpdate)
			{
				data = await _animalTypeRepository.FindAsync(x => x.Id == persist.Id);

				if (data == null) throw new NotFoundException("No animal type found with id given", persist.Id, typeof(Data.Entities.AnimalType));

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
				throw new InvalidOperationException("Failed to persist animal type");

			// Return dto model
			AnimalTypeLookup lookup = new AnimalTypeLookup();
			lookup.Ids = new List<String> { dataId };
			lookup.Fields = fields;
			lookup.Offset = 0;
			lookup.PageSize = 1;

            AuthContext context = _contextBuilder.OwnedFrom(lookup).AffiliatedWith(lookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<AnimalTypeCensor>().Censor([.. lookup.Fields], context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying animal types");

            lookup.Fields = censoredFields;
            return (await _builderFactory.Builder<AnimalTypeBuilder>().Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
										 .Build(await lookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation).CollectAsync(), censoredFields))
										 .FirstOrDefault();
		}

		public async Task Delete(String id) { await this.Delete(new List<String>() { id }); }

		public async Task Delete(List<String> ids)
		{
            if (!await _authorisationService.AuthorizeAsync(Permission.DeleteAnimalTypes))
                throw new ForbiddenException("You are not authorized to delete animal types", typeof(Data.Entities.AnimalType), Permission.DeleteAnimalTypes);

            BreedLookup breedsLookup = new BreedLookup();
            breedsLookup.TypeIds = ids;
            breedsLookup.Fields = new List<String> { nameof(Models.Breed.Breed.Id) };
            breedsLookup.Offset = 1;
            breedsLookup.PageSize = 10000;

            List<Data.Entities.Breed> breeds = await breedsLookup.EnrichLookup(_queryFactory).CollectAsync();
            await _breedService.Value.Delete([.. breeds?.Select(x => x.Id)]);

            await _animalTypeRepository.DeleteAsync(ids);
		}
	}
}