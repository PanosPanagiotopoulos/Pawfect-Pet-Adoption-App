using AutoMapper;
using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Censors;
using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
using Pawfect_Pet_Adoption_App_API.Exceptions;
using Pawfect_Pet_Adoption_App_API.Models.AdoptionApplication;
using Pawfect_Pet_Adoption_App_API.Models.File;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Query;
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
		private readonly IAdoptionApplicationRepository _adoptionApplicationRepository;
		private readonly IMapper _mapper;
        private readonly IQueryFactory _queryFactory;
        private readonly ICensorFactory _censorFactory;
        private readonly AuthContextBuilder _contextBuilder;
        private readonly IBuilderFactory _builderFactory;
        private readonly IConventionService _conventionService;
		private readonly Lazy<IFileService> _fileService;
		private readonly IAuthorisationService _authorisationService;
        private readonly IAuthorisationContentResolver _authorisationContentResolver;
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
            IAuthorisationService authorisationServce,
			IAuthorisationContentResolver authorisationContentResolver,
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
            _authorisationService = authorisationServce;
            _authorisationContentResolver = authorisationContentResolver;
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


                _mapper.Map(persist, data);
				data.UpdatedAt = DateTime.UtcNow;
            }
            else
			{
                if (!await _authorisationService.AuthorizeAsync(Permission.CreateAdoptionApplications))
                    throw new ForbiddenException("Unauthorised access", typeof(Data.Entities.AdoptionApplication), Permission.CreateAdoptionApplications);

                _mapper.Map(persist, data);
				data.Id = null;
				data.CreatedAt = DateTime.UtcNow;
				data.UpdatedAt = DateTime.UtcNow;
			}

			// Set files to permanent
			await this.PersistFiles(persist.AttachedFilesIds, data.AttachedFilesIds);

			if (isUpdate) dataId = await _adoptionApplicationRepository.UpdateAsync(data);
			else dataId = await _adoptionApplicationRepository.AddAsync(data);


			if (String.IsNullOrEmpty(dataId))
				throw new InvalidOperationException("Failed to persist Adoption Application");

			// Return dto model
			AdoptionApplicationLookup lookup = new AdoptionApplicationLookup();
			lookup.Ids = new List<String> { dataId };
			lookup.Fields = fields;
			lookup.Offset = 0;
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
			List<String> affiliatedRolesOfPermission = _authorisationContentResolver.AffiliatedRolesOf(permission);
            
			ClaimsPrincipal claimsPrincipal = _authorisationContentResolver.CurrentPrincipal();
            String userId = _claimsExtractor.CurrentUserId(claimsPrincipal);
            if (!_conventionService.IsValidId(userId)) throw new UnAuthenticatedException("No authenticated user found");

            String userShelterId = await _authorisationContentResolver.CurrentPrincipalShelter();

            foreach (Data.Entities.AdoptionApplication data in datas)
			{
                AdoptionApplicationLookup lookup = new AdoptionApplicationLookup();
                lookup.ShelterIds = new List<String> { data.ShelterId };
                AuthContext authContext =
                    _contextBuilder.OwnedFrom(lookup, data.UserId)
                                   .AffiliatedWith(lookup, affiliatedRolesOfPermission, userShelterId)
                                   .Build();

				if (await _authorisationService.AuthorizeOrOwnedOrAffiliated(authContext, permission))
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
			
			List<Data.Entities.File> attachedFiles = await lookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation).CollectAsync();
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
