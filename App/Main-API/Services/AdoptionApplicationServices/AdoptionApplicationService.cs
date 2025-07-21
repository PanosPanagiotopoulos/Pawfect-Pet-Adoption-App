using AutoMapper;
using Main_API.Builders;
using Main_API.Censors;
using Main_API.Data.Entities.EnumTypes;
using Main_API.Data.Entities.Types.Authorization;
using Main_API.Exceptions;
using Main_API.Models.AdoptionApplication;
using Main_API.Models.File;
using Main_API.Models.Lookups;
using Main_API.Query;
using Main_API.Repositories.Interfaces;
using Main_API.Services.AuthenticationServices;
using Main_API.Services.Convention;
using Main_API.Services.FileServices;
using System.Security.Claims;

namespace Main_API.Services.AdoptionApplicationServices
{
	public class AdoptionApplicationService : IAdoptionApplicationService
	{
		private readonly ILogger<AdoptionApplicationService> _logger;
		private readonly IAdoptionApplicationRepository _adoptionApplicationRepository;
		private readonly IMapper _mapper;
        private readonly IQueryFactory _queryFactory;
        private readonly ICensorFactory _censorFactory;
        private readonly AuthContextBuilder _contextBuilder;
        private readonly IBuilderFactory _builderFactory;
        private readonly IConventionService _conventionService;
		private readonly Lazy<IFileService> _fileService;
		private readonly IAuthorizationService _authorizationService;
        private readonly IAuthorizationContentResolver _authorizationContentResolver;
        private readonly ClaimsExtractor _claimsExtractor;

        public AdoptionApplicationService(
            ILogger<AdoptionApplicationService> logger,
            IAdoptionApplicationRepository adoptionApplicationRepository,
            IMapper mapper,
			IQueryFactory queryFactory,
            ICensorFactory censorFactory,
            AuthContextBuilder contextBuilder,
            IBuilderFactory builderFactory,
            IConventionService conventionService,
            Lazy<IFileService> fileService,
            IAuthorizationService AuthorizationServce,
			IAuthorizationContentResolver AuthorizationContentResolver,
			ClaimsExtractor claimsExtractor)
        {
            _logger = logger;
            _adoptionApplicationRepository = adoptionApplicationRepository;
            _mapper = mapper;
            _queryFactory = queryFactory;
            _censorFactory = censorFactory;
            _contextBuilder = contextBuilder;
            _builderFactory = builderFactory;
            _conventionService = conventionService;
            _fileService = fileService;
            _authorizationService = AuthorizationServce;
            _authorizationContentResolver = AuthorizationContentResolver;
            _claimsExtractor = claimsExtractor;
        }
		public async Task<Models.AdoptionApplication.AdoptionApplication> Persist(AdoptionApplicationPersist persist, List<String> fields)
		{
			Boolean isUpdate =  _conventionService.IsValidId(persist.Id);
            Data.Entities.AdoptionApplication data = new Data.Entities.AdoptionApplication();
			String dataId = String.Empty;
			if (isUpdate)
			{
				data = await _adoptionApplicationRepository.FindAsync(x => x.Id == persist.Id);

				if (data == null) throw new NotFoundException("No adoption application found with id given", persist.Id, typeof(Data.Entities.AdoptionApplication));

				if(!await AuthorisePersistAdoptionApplication(data, Permission.EditAdoptionApplications))
                    throw new ForbiddenException("Unauthorised access", typeof(Data.Entities.AdoptionApplication), Permission.EditAdoptionApplications);

                String userShelterId = await _authorizationContentResolver.CurrentPrincipalShelter();
                if (String.IsNullOrEmpty(userShelterId))
				{
					// Only allow to change: Details , Files
					if (data.Status != persist.Status) throw new ForbiddenException("Not allowed action");
                    if (data.AnimalId != persist.AnimalId) throw new ForbiddenException("Not allowed action");
                    if (data.ShelterId != persist.ShelterId) throw new ForbiddenException("Not allowed action");
                    if (data.AnimalId != persist.AnimalId) throw new ForbiddenException("Not allowed action");
                }


				data.UpdatedAt = DateTime.UtcNow;
            }
            else
			{
                if (!await _authorizationService.AuthorizeAsync(Permission.CreateAdoptionApplications))
                    throw new ForbiddenException("Unauthorised access", typeof(Data.Entities.AdoptionApplication), Permission.CreateAdoptionApplications);

                ClaimsPrincipal claimsPrincipal = _authorizationContentResolver.CurrentPrincipal();
                String userId = _claimsExtractor.CurrentUserId(claimsPrincipal);
                if (!_conventionService.IsValidId(userId)) throw new ForbiddenException("No authenticated user found");

                data.Id = null;
				data.UserId = userId;
				data.CreatedAt = DateTime.UtcNow;
				data.UpdatedAt = DateTime.UtcNow;
			}
			// Set files to permanent
			await this.PersistFiles(persist.AttachedFilesIds, data.AttachedFilesIds);

            _mapper.Map(persist, data);

            if (isUpdate) dataId = await _adoptionApplicationRepository.UpdateAsync(data);
			else dataId = await _adoptionApplicationRepository.AddAsync(data);


			if (String.IsNullOrEmpty(dataId))
				throw new InvalidOperationException("Failed to persist Adoption Application");

			// Return dto model
			AdoptionApplicationLookup lookup = new AdoptionApplicationLookup();
			lookup.Ids = new List<String> { dataId };
			lookup.Fields = fields;
			lookup.Offset = 1;
			lookup.PageSize = 1;

            AuthContext censorContext = _contextBuilder.OwnedFrom(lookup).AffiliatedWith(lookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<AdoptionApplicationCensor>().Censor([.. lookup.Fields], censorContext);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying adoption applications");

            lookup.Fields = censoredFields;
            return (await _builderFactory.Builder<AdoptionApplicationBuilder>().Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
				.Build(await lookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation).CollectAsync(), censoredFields))
				.FirstOrDefault();
		}

		private async Task<Boolean> AuthorisePersistAdoptionApplication(Data.Entities.AdoptionApplication data, String permission)
			=> await AuthorisePersistAdoptionApplication(new List<Data.Entities.AdoptionApplication> { data }, permission);
        private async Task<Boolean> AuthorisePersistAdoptionApplication(List<Data.Entities.AdoptionApplication> datas, String permission)
        {            
			ClaimsPrincipal claimsPrincipal = _authorizationContentResolver.CurrentPrincipal();
            String userId = _claimsExtractor.CurrentUserId(claimsPrincipal);
            if (!_conventionService.IsValidId(userId)) throw new ForbiddenException("No authenticated user found");

            String userShelterId = await _authorizationContentResolver.CurrentPrincipalShelter();

            foreach (Data.Entities.AdoptionApplication data in datas)
			{
                AdoptionApplicationLookup lookup = new AdoptionApplicationLookup();
				lookup.ShelterIds = new List<String> { data.ShelterId };
                lookup.UserIds = new List<String> { userId };
                AuthContext authContext =
                    _contextBuilder.OwnedFrom(lookup, data.UserId)
                                   .AffiliatedWith(lookup)
                                   .Build();

				if (await _authorizationService.AuthorizeOrOwnedOrAffiliated(authContext, permission))
					return true;
            }

			return false;
        }

        private async Task PersistFiles(List<String> attachedFilesIds, List<String> currentFileIds)
		{
            // Make null lto an empty list so that we can delete all current file Ids
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
			lookup.Offset = 1;
			lookup.PageSize = attachedFilesIds.Count;
			
			List<Data.Entities.File> attachedFiles = await lookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermission).CollectAsync();
			if (attachedFiles == null || attachedFiles.Count == 0)
			{
				_logger.LogError("Failed to saved attached files. No return from query");
				return;
			}

			List<FilePersist> persistModels = new List<FilePersist>();

			ClaimsPrincipal claimsPrincipal = _authorizationContentResolver.CurrentPrincipal();
			String userId = _claimsExtractor.CurrentUserId(claimsPrincipal);

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

            AdoptionApplicationLookup lookup = new AdoptionApplicationLookup();
            lookup.Ids = ids;
            lookup.Offset = 1;
            lookup.PageSize = 10000;
			lookup.Fields = new List<String> {
												nameof(Models.AdoptionApplication.AdoptionApplication.Id),
											    nameof(Models.AdoptionApplication.AdoptionApplication.Shelter) + "." + nameof(Models.Shelter.Shelter.Id),
                                                nameof(Models.AdoptionApplication.AdoptionApplication.User) + "." + nameof(Models.User.User.Id)
											 };

			List<Data.Entities.AdoptionApplication> datas = await lookup.EnrichLookup(_queryFactory).CollectAsync();

			if (!await this.AuthorisePersistAdoptionApplication(datas, Permission.DeleteAdoptionApplications))
                throw new ForbiddenException("Unauthorised access", typeof(Data.Entities.AdoptionApplication), Permission.DeleteAdoptionApplications);

            FileLookup attachedFilesLookup = new FileLookup();
            attachedFilesLookup.OwnerIds = [..datas.Select(adp => adp.UserId)];
            attachedFilesLookup.Fields = new List<String> { nameof(Models.File.File.Id) };
            attachedFilesLookup.Offset = 1;
            attachedFilesLookup.PageSize = 10000;

            List<Data.Entities.File> attachedFiles = await attachedFilesLookup.EnrichLookup(_queryFactory).CollectAsync();
            await _fileService.Value.Delete([.. attachedFiles?.Select(x => x.Id)]);

            await _adoptionApplicationRepository.DeleteAsync(ids);
		}
	}
}
