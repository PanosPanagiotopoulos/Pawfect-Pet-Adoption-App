using AutoMapper;
using Pawfect_API.Builders;
using Pawfect_API.Censors;
using Pawfect_API.Data.Entities;
using Pawfect_API.Data.Entities.EnumTypes;
using Pawfect_API.Data.Entities.Types.Authorization;
using Pawfect_API.Exceptions;
using Pawfect_API.Models.Animal;
using Pawfect_API.Models.File;
using Pawfect_API.Models.Lookups;
using Pawfect_API.Query;
using Pawfect_API.Query.Queries;
using Pawfect_API.Repositories.Interfaces;
using Pawfect_API.Services.AdoptionApplicationServices;
using Pawfect_API.Services.AuthenticationServices;
using Pawfect_API.Services.Convention;
using Pawfect_API.Services.FileServices;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;

namespace Pawfect_API.Services.AnimalServices
{
	public class AnimalService : IAnimalService
	{
		private readonly ILogger<AnimalService> _logger;
		private readonly IAnimalRepository _animalRepository;
		private readonly IMapper _mapper;
		private readonly IConventionService _conventionService;
        private readonly ClaimsExtractor _claimsExtractor;
        private readonly IAuthorizationContentResolver _authorizationContentResolver;
        private readonly Lazy<IFileService> _fileService;
        private readonly Lazy<IAdoptionApplicationService> _adoptionApplicationService;
        private readonly ICensorFactory _censorFactory;
        private readonly AuthContextBuilder _contextBuilder;
        private readonly IQueryFactory _queryFactory;
        private readonly IBuilderFactory _builderFactory;
        private readonly IAuthorizationService _authorizationService;

        public AnimalService
			(
				ILogger<AnimalService> logger,
				IAnimalRepository animalRepository,
				IMapper mapper,
				Lazy<IFileService> fileService,
                Lazy<IAdoptionApplicationService> adoptionApplicationService,
				ICensorFactory censorFactory,
                AuthContextBuilder contextBuilder,
                IQueryFactory queryFactory,
                IBuilderFactory builderFactory,
                IAuthorizationService AuthorizationService,
                IConventionService conventionService,
				ClaimsExtractor claimsExtractor,
				IAuthorizationContentResolver AuthorizationContentResolver

            )
		{
			_logger = logger;
			_animalRepository = animalRepository;
			_mapper = mapper;
			_conventionService = conventionService;
            _claimsExtractor = claimsExtractor;
            _authorizationContentResolver = AuthorizationContentResolver;
            _fileService = fileService;
            _adoptionApplicationService = adoptionApplicationService;
            _censorFactory = censorFactory;
            _contextBuilder = contextBuilder;
            _authorizationService = AuthorizationService;
            _queryFactory = queryFactory;
            _builderFactory = builderFactory;
		}

        public async Task<Models.Animal.Animal?> Persist(AnimalPersist persist, List<String> fields)
            => (await this.PersistBatch(new List<AnimalPersist>() { persist }, fields))?.FirstOrDefault();

        public async Task<List<Models.Animal.Animal>> PersistBatch(List<AnimalPersist> models, List<String> fields)
        {
            Boolean isUpdate = models.Select(model => model.Id).All(_conventionService.IsValidId);

            String userShelterId = await _authorizationContentResolver.CurrentPrincipalShelter();
            if (!_conventionService.IsValidId(userShelterId)) throw new ForbiddenException("A non-shelter cannot create animals");

            List<Data.Entities.Animal> datas = new List<Data.Entities.Animal>();
            List<String> dataIds = new List<String>();
            List<String> prevFileIds = new List<String>();
            AnimalQuery q = _queryFactory.Query<AnimalQuery>();
            q.Offset = 0;
            q.PageSize = models.Count;
            if (isUpdate)
            {
                q.Ids = models.Select(m => m.Id).ToList();

				datas = await q.CollectAsync();

                if (!await this.AuthorizeAnimalPersist(models, datas.Select(Double => Double.ShelterId).ToList()))
                    throw new ForbiddenException("You are not authorized to edit animals", typeof(Data.Entities.Animal), Permission.EditAnimals);

                if (datas == null || datas.Count != models.Count) throw new NotFoundException("Not all animals found to persist", null, typeof(Data.Entities.Animal));

                prevFileIds.AddRange(datas.Where(Double => Double.PhotosIds != null).SelectMany(Double => Double.PhotosIds));

				foreach (AnimalPersist animalPersist in models)
				{
					Data.Entities.Animal animal = datas.Where(data => data.Id.Equals(animalPersist.Id)).FirstOrDefault();
                    if (!animal.ShelterId.Equals(userShelterId)) throw new InvalidOperationException("Cannot updatea an animals shelter");

                    _mapper.Map(animalPersist, animal);
                    animal.PhotosIds = animalPersist.AttachedPhotosIds;
					animal.UpdatedAt = DateTime.UtcNow;
				}
            }
            else
            {
                if (!await _authorizationService.AuthorizeAsync(Permission.CreateAnimals))
                    throw new ForbiddenException("You are not authorized to create animals", typeof(Data.Entities.Animal), Permission.CreateAnimals);

                foreach (AnimalPersist animalPersist in models)
                {
                    Data.Entities.Animal animal = _mapper.Map<Data.Entities.Animal>(animalPersist);
                    animal.Id = null;
                    animal.PhotosIds = animalPersist.AttachedPhotosIds;
                    animal.ShelterId = userShelterId;
                    animal.CreatedAt = DateTime.UtcNow;
                    animal.UpdatedAt = DateTime.UtcNow;

                    datas.Add(animal);
                }
            }

			await this.PersistFiles(
				models.Where(x => x.AttachedPhotosIds != null && x.AttachedPhotosIds.Count > 0).SelectMany(x => x.AttachedPhotosIds).ToList(),
				prevFileIds
			);

            if (isUpdate) dataIds = await _animalRepository.UpdateManyAsync(datas);
            else dataIds = await _animalRepository.AddManyAsync(datas);

            if (dataIds == null || dataIds.Count != models.Count)
                throw new InvalidOperationException("Failed to persist all animals");

            // Return dto model
            AnimalLookup lookup = new AnimalLookup();
            lookup.Ids = dataIds;
            lookup.Fields = fields.Concat(new List<String>() { nameof(Models.Animal.Animal.Name) }).ToList();
            lookup.Offset = 0;
            lookup.PageSize = models.Count;

            AuthContext context = _contextBuilder.OwnedFrom(lookup).AffiliatedWith(lookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<AnimalCensor>().Censor([.. lookup.Fields], context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying animals");
            lookup.Fields = censoredFields;

			return await _builderFactory.Builder<AnimalBuilder>().Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation).Build(await lookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation).CollectAsync(), censoredFields);
        }

        private async Task<Boolean> AuthorizeAnimalPersist(List<AnimalPersist> models, List<String> shelterIds = null)
        {
            ClaimsPrincipal claimsPrincipal = _authorizationContentResolver.CurrentPrincipal();
            String userId = _claimsExtractor.CurrentUserId(claimsPrincipal);

            String shelterId = await _authorizationContentResolver.CurrentPrincipalShelter();
            if (!_conventionService.IsValidId(shelterId)) throw new ForbiddenException("Cannot delete if not a shelter");

            OwnedResource ownership = new OwnedResource(userId, new OwnedFilterParams(new AnimalLookup()));

            AnimalLookup affiliation = new AnimalLookup();
            if (shelterId != null)
                affiliation.ShelterIds = shelterIds;

            AffiliatedResource affiliatedResource = new AffiliatedResource(new AffiliatedFilterParams(affiliation));

            return await _authorizationService.AuthorizeOrOwnedOrAffiliated(_contextBuilder.OwnedFrom(ownership).AffiliatedWith(affiliation).Build(), Permission.EditAnimals);
        }

        private async Task PersistFiles(List<String> attachedFilesIds, List<String> currentFileIds)
		{
			// Make nul lto an empty list so that we can delete all current file Ids
			if (attachedFilesIds == null) { attachedFilesIds = new List<String>(); }

            if (currentFileIds != null)
			{
				List<String> diff = [..currentFileIds.Except(attachedFilesIds)];

				// Else delete the ones that remains since they where deleted from the file id list
				if (diff.Count != 0) await _fileService.Value.Delete(diff);
			}

			// Is empty, means it got deleted, so no need to query for persisting
			if (attachedFilesIds.Count == 0) return;

			FileLookup lookup = new FileLookup();
			lookup.Ids = attachedFilesIds;
			lookup.Offset = 0;
			lookup.PageSize = attachedFilesIds.Count;

			List<Data.Entities.File> attachedFiles = await lookup.EnrichLookup(_queryFactory).CollectAsync();
			if (attachedFiles == null || attachedFiles.Count == 0)
			{
				_logger.LogError("Failed to saved attached files. No return from query");
				return;
			}

            ClaimsPrincipal claimsPrincipal = _authorizationContentResolver.CurrentPrincipal();
            String userId = _claimsExtractor.CurrentUserId(claimsPrincipal);

            List<FilePersist> persistModels = new List<FilePersist>();
			foreach (Data.Entities.File file in attachedFiles)
			{
				file.FileSaveStatus = FileSaveStatus.Permanent;
				file.OwnerId = userId;
				persistModels.Add(_mapper.Map<FilePersist>(file));
			}

			await _fileService.Value.Persist
			(
				persistModels,
				new List<String>() { nameof(Models.File.File.Id) }
			);
		}

		public async Task Delete(String id) { await this.Delete(new List<String>() { id }); }

		public async Task Delete(List<String> ids)
		{
            if (ids == null || !ids.Any()) return;

            String shelterId = await _authorizationContentResolver.CurrentPrincipalShelter();
            if (!_conventionService.IsValidId(shelterId)) throw new ForbiddenException("Cannot delete if not a shelter");

            AnimalLookup lookup = new AnimalLookup();
            lookup.Ids = ids;
            lookup.ShelterIds = [shelterId];
            lookup.Fields = new List<String> { nameof(Models.Animal.Animal.Id), String.Join('.', nameof(Models.Animal.Animal.AttachedPhotos), nameof(Models.File.File.Id)) };
            lookup.Offset = 0;
            lookup.PageSize = 10000;

            List<Pawfect_API.Data.Entities.Animal> animals = [.. await lookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation).CollectAsync()];

            if (!await _authorizationService.AuthorizeAsync(Permission.DeleteAnimals)
                && (animals == null || !animals.Any())) throw new ForbiddenException("You are not authorized to delete animals", typeof(Data.Entities.Animal), Permission.DeleteAnimals);

            await _adoptionApplicationService.Value.DeleteFromAnimals([.. animals.Select(x => x.Id)]);

            await _fileService.Value.Delete([..animals.Where(animal => animal.PhotosIds != null).SelectMany(animal => animal.PhotosIds)]);

			await _animalRepository.DeleteManyAsync(ids);
		}
	}
}