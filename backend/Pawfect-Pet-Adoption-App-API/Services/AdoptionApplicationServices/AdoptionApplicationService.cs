using AutoMapper;
using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
using Pawfect_Pet_Adoption_App_API.Exceptions;
using Pawfect_Pet_Adoption_App_API.Models.AdoptionApplication;
using Pawfect_Pet_Adoption_App_API.Models.File;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Query;
using Pawfect_Pet_Adoption_App_API.Query.Queries;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;
using Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices;
using Pawfect_Pet_Adoption_App_API.Services.Convention;
using Pawfect_Pet_Adoption_App_API.Services.FileServices;
using System.Security.Claims;

namespace Pawfect_Pet_Adoption_App_API.Services.AdoptionApplicationServices
{
	public class AdoptionApplicationService : IAdoptionApplicationService
	{
		private readonly ILogger<AdoptionApplicationService> _logger;
		private readonly AdoptionApplicationQuery _adoptionApplicationQuery;
		private readonly AdoptionApplicationBuilder _adoptionApplicationBuilder;
		private readonly IAdoptionApplicationRepository _adoptionApplicationRepository;
		private readonly IMapper _mapper;
        private readonly IQueryFactory _queryFactory;
        private readonly IBuilderFactory _builderFactory;
        private readonly IConventionService _conventionService;
		private readonly Lazy<IFileService> _fileService;
		private readonly FileQuery _fileQuery;
		private readonly IAuthorisationService _authorisationService;
        private readonly IAuthorisationContentResolver _authorisationContentResolver;
        private readonly ClaimsExtractor _claimsExtractor;

        public AdoptionApplicationService(
            ILogger<AdoptionApplicationService> logger,
            IAdoptionApplicationRepository adoptionApplicationRepository,
            IMapper mapper,
			IQueryFactory queryFactory,
            IBuilderFactory builderFactory,
            IConventionService conventionService,
            Lazy<IFileService> fileService,
            IAuthorisationService authorisationServce,
			IAuthorisationContentResolver authorisationContentResolver,
            ClaimsExtractor claimsExtractor)
        {
            _logger = logger;
            _adoptionApplicationRepository = adoptionApplicationRepository;
            _mapper = mapper;
            _queryFactory = queryFactory;
            _builderFactory = builderFactory;
            _conventionService = conventionService;
            _fileService = fileService;
            _authorisationService = authorisationServce;
            _authorisationContentResolver = authorisationContentResolver;
            _claimsExtractor = claimsExtractor;
        }
		public async Task<AdoptionApplicationDto> Persist(AdoptionApplicationPersist persist, List<String> fields)
		{
			Boolean isUpdate =  _conventionService.IsValidId(persist.Id);
			AdoptionApplication data = new AdoptionApplication();
			String dataId = String.Empty;
			if (isUpdate)
			{
				data = await _adoptionApplicationRepository.FindAsync(x => x.Id == persist.Id);

				if (data == null) throw new InvalidDataException("No entity found with id given");

                ClaimsPrincipal claimsPrincipal = _authorisationContentResolver.CurrentPrincipal();
                String userId = _claimsExtractor.CurrentUserId(claimsPrincipal);
                if (!_conventionService.IsValidId(userId)) throw new Exception("No user found");

                //OwnedResource resource = new OwnedResource(userId, typeof(AdoptionApplication));
                //if (!await _authorisationService.AuthorizeOrOwnedAsync(resource, data.Id, Permission.EditAdoptionApplications))
                //    throw new ForbiddenException("Unauthorised access", resource.ResourceType, Permission.EditAdoptionApplications);

                _mapper.Map(persist, data);
				data.UpdatedAt = DateTime.UtcNow;
            }
            else
			{
                if (!await _authorisationService.AuthorizeAsync(Permission.CreateAdoptionApplications))
                    throw new ForbiddenException("Unauthorised access", Permission.CreateAdoptionApplications);

                _mapper.Map(persist, data);
				data.Id = null;
				data.CreatedAt = DateTime.UtcNow;
				data.UpdatedAt = DateTime.UtcNow;
			}

			// Set files to permanent
			//await this.PersistFiles(persist.AttachedFilesIds, data.AttachedFilesIds, new OwnedResource(data.UserId, typeof(Data.Entities.File)));

			if (isUpdate) dataId = await _adoptionApplicationRepository.UpdateAsync(data);
			else dataId = await _adoptionApplicationRepository.AddAsync(data);


			if (String.IsNullOrEmpty(dataId))
			{
				throw new InvalidOperationException("Αποτυχία κατα το Persisting");
			}

			// Return dto model
			AdoptionApplicationLookup lookup = new AdoptionApplicationLookup();
			lookup.Ids = new List<String> { dataId };
			lookup.Fields = fields;
			lookup.Offset = 0;
			lookup.PageSize = 1;

			return (await _builderFactory.Builder<AdoptionApplicationBuilder>().BuildDto(await lookup.EnrichLookup(_queryFactory).CollectAsync(), fields)).FirstOrDefault();
		}

		private async Task PersistFiles(List<String> attachedFilesIds, List<String> currentFileIds, OwnedResource fileOwner)
		{
            //if (!await _authorisationService.AuthorizeAsync(fileOwner, attachedFilesIds, Permission.EditFiles))
            //    throw new ForbiddenException("Unauthorised access", Permission.EditFiles);

            // Make null lto an empty list so that we can delete all current file Ids
            if (attachedFilesIds == null) { attachedFilesIds = new List<String>(); }

			if (currentFileIds != null)
			{
				List<String> diff = [..currentFileIds.Except(attachedFilesIds)];

				// If no difference with current , return
				if (diff.Count == 0 && currentFileIds.Count == attachedFilesIds.Count) { return; }

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
			if (attachedFiles == null || !attachedFiles.Any())
			{
				_logger.LogError("Failed to saved attached files. No return from query");
				return;
			}

			List<FilePersist> persistModels = new List<FilePersist>();
			foreach (Data.Entities.File file in attachedFiles)
			{
				file.FileSaveStatus = FileSaveStatus.Permanent;
				persistModels.Add(_mapper.Map<FilePersist>(file));
			}

			await _fileService.Value.Persist
			(
				persistModels, 
				new List<String>() { nameof(FileDto.Id) }
			);
		}

		public async Task Delete(String id) { await this.Delete(new List<String>() { id }); }

		public async Task Delete(List<String> ids)
		{
            ClaimsPrincipal claimsPrincipal = _authorisationContentResolver.CurrentPrincipal();
			String userId = _claimsExtractor.CurrentUserId(claimsPrincipal);
			if (!_conventionService.IsValidId(userId)) throw new Exception("No user found");

			//OwnedResource ownedResource = new OwnedResource(userId, typeof(AdoptionApplication));
   //         if (!await _authorisationService.AuthorizeAsync(ownedResource, ids, Permission.DeleteAdoptionApplications))
   //             throw new ForbiddenException("Unauthorised access", ownedResource.ResourceType, Permission.DeleteAdoptionApplications);

            FileLookup lookup = new FileLookup();
			lookup.OwnerIds = ids;
			lookup.Fields = new List<String> { nameof(AdoptionApplicationDto.Id) };
			lookup.Offset = 0;
			lookup.PageSize = 50;

			List<Data.Entities.File> attachedFiles = await lookup.EnrichLookup(_queryFactory).CollectAsync();
			await _fileService.Value.Delete(attachedFiles?.Select(x => x.Id).ToList());


			await _adoptionApplicationRepository.DeleteAsync(ids);
		}
	}
}
