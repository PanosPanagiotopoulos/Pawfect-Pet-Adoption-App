﻿using Amazon.S3.Transfer;
using AutoMapper;

using Main_API.Builders;
using Main_API.Censors;
using Main_API.Data.Entities;
using Main_API.Data.Entities.EnumTypes;
using Main_API.Data.Entities.Types.Authorization;
using Main_API.Exceptions;
using Main_API.Models.Animal;
using Main_API.Models.File;
using Main_API.Models.Lookups;
using Main_API.Query;
using Main_API.Query.Queries;
using Main_API.Repositories.Implementations;
using Main_API.Repositories.Interfaces;
using Main_API.Services.AuthenticationServices;
using Main_API.Services.BreedServices;
using Main_API.Services.Convention;
using Main_API.Services.FileServices;
using System.Security.Claims;

namespace Main_API.Services.AnimalServices
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
            _censorFactory = censorFactory;
            _contextBuilder = contextBuilder;
            _authorizationService = AuthorizationService;
            _queryFactory = queryFactory;
            _builderFactory = builderFactory;
		}
		public async Task<Models.Animal.Animal?> Persist(AnimalPersist persist, List<String> fields)
		{
			Boolean isUpdate = _conventionService.IsValidId(persist.Id);
            Data.Entities.Animal data = new Data.Entities.Animal();
			String dataId = String.Empty;

			if (!await this.AuthoriseAnimalPersist(persist, Permission.EditAnimals))
                throw new ForbiddenException("You are not authorized to edit animals", typeof(Data.Entities.Animal), Permission.EditAnimals);

            if (isUpdate)
			{
				data = await _animalRepository.FindAsync(x => x.Id == persist.Id);

				if (data == null) throw new NotFoundException("No animal found to persist", persist.Id, typeof(Data.Entities.Animal));

				_mapper.Map(persist, data);
				data.UpdatedAt = DateTime.UtcNow;
			}
			else
			{
				_mapper.Map(persist, data);
				data.Id = null;
				data.CreatedAt = DateTime.UtcNow;
				data.UpdatedAt = DateTime.UtcNow;
			}

			await this.PersistFiles(persist.AttachedPhotosIds, data.PhotosIds);

			if (isUpdate) dataId = await _animalRepository.UpdateAsync(data);
			else dataId = await _animalRepository.AddAsync(data);

			if (String.IsNullOrEmpty(dataId))
				throw new InvalidOperationException("Failed to persist animal");

			// Return dto model
			AnimalLookup lookup = new AnimalLookup();
			lookup.Ids = new List<String> { dataId };
			lookup.Fields = fields;
			lookup.Offset = 0;
			lookup.PageSize = 1;

            AuthContext context = _contextBuilder.OwnedFrom(lookup).AffiliatedWith(lookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<AnimalCensor>().Censor([.. lookup.Fields], context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying animals");

            lookup.Fields = censoredFields;
            return (await _builderFactory.Builder<AnimalBuilder>().Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
										.Build(await lookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation).CollectAsync(), censoredFields))
										.FirstOrDefault();
		}

		private async Task<Boolean> AuthoriseAnimalPersist(AnimalPersist animal, String permission)
		{
			String userShelterId = await _authorizationContentResolver.CurrentPrincipalShelter();
			if (!_conventionService.IsValidId(userShelterId) || !animal.ShelterId.Equals(userShelterId)) return false;

            return await _authorizationService.AuthorizeAsync(permission);
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
			lookup.Fields = new List<String> { "*" };
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
			if (!await this.AuthoriseAnimalDeletion(ids))
                throw new ForbiddenException("You are not authorized to delete animals", typeof(Data.Entities.Animal), Permission.DeleteAnimals);

            FileLookup fLookup = new FileLookup();
            fLookup.OwnerIds = ids;
            fLookup.Fields = new List<String> { nameof(Models.File.File.Id) };
            fLookup.Offset = 0;
            fLookup.PageSize = 10000;

			List<Data.Entities.File> attachedFiles = await fLookup.EnrichLookup(_queryFactory).CollectAsync();
			await _fileService.Value.Delete([..attachedFiles?.Select(x => x.Id)]);

			await _animalRepository.DeleteAsync(ids);
		}

		private async Task<Boolean> AuthoriseAnimalDeletion(List<String> animalIds)
		{
			if (await _authorizationService.AuthorizeAsync(Permission.DeleteAnimals)) return true;

            String shelterId = await _authorizationContentResolver.CurrentPrincipalShelter();
			if (!_conventionService.IsValidId(shelterId)) return false;

            AnimalLookup lookup = new AnimalLookup();
            lookup.Ids = animalIds;
            lookup.ShelterIds = [shelterId];
            lookup.Fields = new List<String> { nameof(Models.Animal.Animal.Id) };
            lookup.Offset = 1;
            lookup.PageSize = 10000;

            List<String> allowedAnimalsToDelete = [.. (await lookup.EnrichLookup(_queryFactory).CollectAsync())?.Select(animal => animal.Id)];
			if (allowedAnimalsToDelete == null) return false;

            List<String> filteredAnimalDeletions = [.. animalIds.Except(allowedAnimalsToDelete)];

			return filteredAnimalDeletions.Count == 0;
        } 
	}
}