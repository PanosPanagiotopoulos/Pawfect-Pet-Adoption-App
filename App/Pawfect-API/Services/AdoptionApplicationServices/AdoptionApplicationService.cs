using AutoMapper;
using Microsoft.Extensions.Options;
using Pawfect_API.Builders;
using Pawfect_API.Censors;
using Pawfect_API.Data.Entities.EnumTypes;
using Pawfect_API.Data.Entities.Types.Authorization;
using Pawfect_API.Exceptions;
using Pawfect_API.Models.AdoptionApplication;
using Pawfect_API.Models.File;
using Pawfect_API.Models.Lookups;
using Pawfect_API.Models.Notification;
using Pawfect_API.Query;
using Pawfect_API.Repositories.Interfaces;
using Pawfect_API.Services.AuthenticationServices;
using Pawfect_API.Services.Convention;
using Pawfect_API.Services.FileServices;
using Pawfect_API.Services.NotificationServices;
using Pawfect_API.Data.Entities.Types.Apis;
using Pawfect_API.DevTools;
using System.Security.Claims;

namespace Pawfect_API.Services.AdoptionApplicationServices
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
        private readonly INotificationApiClient _notificationApiClient;
        private readonly IUserRepository _userRepository;
        private readonly IAnimalRepository _animalRepository;
        private readonly IShelterRepository _shelterRepository;
        private readonly NotificationApiConfig _notificationConfig;
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
            IAuthorizationContentResolver authorizationContentResolver,
            INotificationApiClient notificationApiClient,
            IUserRepository userRepository,
            IAnimalRepository animalRepository,
            IOptions<NotificationApiConfig> notificationOptions, 
            IShelterRepository shelterRepository,
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
            _authorizationContentResolver = authorizationContentResolver;
            _notificationApiClient = notificationApiClient;
            _userRepository = userRepository;
            _animalRepository = animalRepository;
            _shelterRepository = shelterRepository;
            _notificationConfig = notificationOptions.Value;
            _claimsExtractor = claimsExtractor;
        }

        #region Persist
        public async Task<Models.AdoptionApplication.AdoptionApplication> Persist(AdoptionApplicationPersist persist, List<String> fields)
        {
            Boolean isUpdate = _conventionService.IsValidId(persist.Id);
            Boolean statusChanged = false;
            Data.Entities.AdoptionApplication data = new Data.Entities.AdoptionApplication();
            List<String> prevFileIds = null;
            String dataId = String.Empty;
            if (isUpdate)
            {
                data = await _adoptionApplicationRepository.FindAsync(x => x.Id == persist.Id);

                if (data == null) throw new NotFoundException("No adoption application found with id given", persist.Id, typeof(Data.Entities.AdoptionApplication));

                if (!await AuthoriseAdoptionApplication(data, Permission.EditAdoptionApplications))
                    throw new ForbiddenException("Unauthorised access", typeof(Data.Entities.AdoptionApplication), Permission.EditAdoptionApplications);

                if (data.Status == ApplicationStatus.Approved) throw new InvalidOperationException("Cannot change accepted application");

                String userShelterId = await _authorizationContentResolver.CurrentPrincipalShelter();
                if (String.IsNullOrEmpty(userShelterId))
                {
                    // Only allow to change: Details , Files
                    if (data.Status != persist.Status) throw new ForbiddenException("Not allowed action");
                    if (data.AnimalId != persist.AnimalId) throw new ForbiddenException("Not allowed action");
                    if (data.ShelterId != persist.ShelterId) throw new ForbiddenException("Not allowed action");
                }

                if (await _animalRepository.ExistsAsync(animal => animal.Id == data.AnimalId && animal.AdoptionStatus == AdoptionStatus.Adopted))
                    throw new InvalidOperationException("Animal already is adopted. We are very sorry for the inconvinience");                   

                prevFileIds = data.AttachedFilesIds;

                data.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                if (!await _authorizationService.AuthorizeAsync(Permission.CreateAdoptionApplications))
                    throw new ForbiddenException("Unauthorised access", typeof(Data.Entities.AdoptionApplication), Permission.CreateAdoptionApplications);

                ClaimsPrincipal claimsPrincipal = _authorizationContentResolver.CurrentPrincipal();
                String userId = _claimsExtractor.CurrentUserId(claimsPrincipal);
                if (!_conventionService.IsValidId(userId)) throw new ForbiddenException("No authenticated user found");

                if (!String.IsNullOrEmpty(await _authorizationContentResolver.CurrentPrincipalShelter()))
                    throw new InvalidOperationException("Shelter cannot adopt an animal");

                if (!String.IsNullOrEmpty(await this.AdoptionRequestExists(persist.AnimalId)))
                    throw new InvalidOperationException("You have already tried to adopt this animal");

                if (await _animalRepository.ExistsAsync(animal => animal.Id == data.AnimalId && animal.AdoptionStatus == AdoptionStatus.Adopted))
                    throw new InvalidOperationException("Cannot adopt already adopted animal");

                data.Id = null;
                data.UserId = userId;
                data.CreatedAt = DateTime.UtcNow;
                data.UpdatedAt = DateTime.UtcNow;
            }

            statusChanged = isUpdate && persist.Status != data.Status;

            _mapper.Map(persist, data);

            // Set files to permanent
            await this.PersistFiles(persist.AttachedFilesIds, prevFileIds, dataId);

            if (isUpdate) dataId = await _adoptionApplicationRepository.UpdateAsync(data);
            else dataId = await _adoptionApplicationRepository.AddAsync(data);

            if (String.IsNullOrEmpty(dataId))
                throw new InvalidOperationException("Failed to persist Adoption Application");

            data.Id = dataId;

            await this.NotifyAffiated(data, statusChanged, isUpdate);

            // Make animal flag set to "Adopted"
            if (persist.Status == ApplicationStatus.Approved)
            {
                Data.Entities.Animal animal = await _animalRepository.FindAsync(animal => animal.Id == data.AnimalId);
                animal.AdoptionStatus = AdoptionStatus.Adopted;
                animal.UpdatedAt = DateTime.UtcNow;
                if (String.IsNullOrEmpty(await _animalRepository.UpdateAsync(animal)))
                {
                    _logger.LogError("Failed to update availability of the animal after beeing adopted");
                    throw new InvalidOperationException("Failed to update availability of the animal after beeing adopted");
                }

                await this.RejectOtherAdoptersRequests(data.AnimalId, data.ShelterId);
            }

            // Return dto model
            AdoptionApplicationLookup lookup = new AdoptionApplicationLookup();
            lookup.Ids = new List<String> { dataId };
            lookup.Fields = fields;
            lookup.Offset = 0;
            lookup.PageSize = 1;

            AuthContext censorContext = _contextBuilder.OwnedFrom(lookup).AffiliatedWith(lookup, null, data.ShelterId).Build();
            List<String> censoredFields = await _censorFactory.Censor<AdoptionApplicationCensor>().Censor([.. lookup.Fields], censorContext);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying adoption applications");

            lookup.Fields = censoredFields;
            return (await _builderFactory.Builder<AdoptionApplicationBuilder>().Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
                .Build([data], censoredFields))
                .FirstOrDefault();
        }

        private async Task<Boolean> AuthoriseAdoptionApplication(Data.Entities.AdoptionApplication data, String permission)
            => await AuthoriseAdoptionApplication(new List<Data.Entities.AdoptionApplication> { data }, permission);
        private async Task<Boolean> AuthoriseAdoptionApplication(List<Data.Entities.AdoptionApplication> datas, String permission)
        {
            ClaimsPrincipal claimsPrincipal = _authorizationContentResolver.CurrentPrincipal();
            String userId = _claimsExtractor.CurrentUserId(claimsPrincipal);
            if (!_conventionService.IsValidId(userId)) throw new ForbiddenException("No authenticated user found");

            String userShelterId = await _authorizationContentResolver.CurrentPrincipalShelter();

            foreach (Data.Entities.AdoptionApplication data in datas)
            {
                AdoptionApplicationLookup lookup = new AdoptionApplicationLookup();
                lookup.Ids = new List<String> { data.Id };
                AuthContext authContext =
                    _contextBuilder.OwnedFrom(lookup, data.UserId)
                                   .AffiliatedWith(lookup, null, data.ShelterId)
                                   .Build();

                if (await _authorizationService.AuthorizeOrOwnedOrAffiliated(authContext, permission))
                    return true;
            }

            return false;
        }

        private async Task RejectOtherAdoptersRequests(String animalId, String shelterId)
        {
            // Reject all applications for this animal that are pending since , if one got accepted then the rest should be rejected
            AdoptionApplicationLookup lookup = new AdoptionApplicationLookup();
            lookup.AnimalIds = new List<String> { animalId };
            lookup.ShelterIds = new List<String> { shelterId };
            lookup.Status = new List<ApplicationStatus>() { ApplicationStatus.Pending };
            lookup.Offset = 0;
            lookup.PageSize = 10000;
            
            List<Data.Entities.AdoptionApplication> datas = await lookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation).CollectAsync();
            if (datas == null || !datas.Any()) return;
           
            foreach (Data.Entities.AdoptionApplication data in datas)
            {
                data.Status = ApplicationStatus.Rejected;
                data.UpdatedAt = DateTime.UtcNow;
            }

            if ((await _adoptionApplicationRepository.UpdateManyAsync(datas)).Count != datas.Count)
            {
                _logger.LogError("Failed to reject other adoption applications after one beeing accepted");
                throw new Exception("Failed to reject connected adoption applications");
            }

            // Notify the adopters for the change in their application status
            await NotifyRejected(datas);

        }

        private async Task PersistFiles(List<String> attachedFilesIds, List<String> currentFileIds, String applicationId)
        {
            // Make null lto an empty list so that we can delete all current file Ids
            if (attachedFilesIds == null) { attachedFilesIds = new List<String>(); }

            if (currentFileIds != null)
            {
                List<String> diff = [.. currentFileIds.Except(attachedFilesIds)];

                // Else delete the ones that remains since they where deleted from the file id list
                if (diff.Count != 0) await _fileService.Value.Delete(diff);
            }

            // Is empty, means it got deleted, so no need to query for persisting
            if (attachedFilesIds.Count == 0) return;

            FileLookup lookup = new FileLookup();
            lookup.Ids = attachedFilesIds;
            lookup.Offset = 0;
            lookup.PageSize = attachedFilesIds.Count;

            List<Data.Entities.File> attachedFiles = await lookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation).CollectAsync();
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
                file.ContextId = applicationId;
                file.ContextType = nameof(Data.Entities.AdoptionApplication);
                persistModels.Add(_mapper.Map<FilePersist>(file));
            }

            await _fileService.Value.Persist
            (
                persistModels,
                new List<String>() { nameof(Models.File.File.Id) }
            );
        }

        #endregion

        #region Notifiers
        private async Task NotifyAffiated(Data.Entities.AdoptionApplication data, Boolean statusChanged, Boolean isUpdate)
        {
            if (String.IsNullOrEmpty(data.Id)) throw new InvalidOperationException("No adoption application id found");

            List<NotificationEvent> generatedEvents = new List<NotificationEvent>();

            Data.Entities.User user = await _userRepository.FindAsync(user => user.Id == data.UserId, [nameof(Models.User.User.FullName)]);
            String userFirstName = UserDataHelper.GetFirstNameFormatted(user.FullName);

            Data.Entities.Shelter receivingShelter = await _shelterRepository.FindAsync(shelter => shelter.Id == data.ShelterId, new List<String> { nameof(Data.Entities.Shelter.UserId), nameof(Data.Entities.Shelter.ShelterName) });
            Data.Entities.Animal referedAnimal = await _animalRepository.FindAsync(animal => animal.Id == data.AnimalId, new List<String> { nameof(Data.Entities.Animal.Name) });

            Boolean changeMadeByUser = String.IsNullOrEmpty(await _authorizationContentResolver.CurrentPrincipalShelter());

            if (!isUpdate)
            {
                // Send only adoption application received notification on shelter
                NotificationEvent applicationReceivedEvent = new NotificationEvent()
                {
                    UserId = receivingShelter.UserId,
                    TeplateId = _notificationConfig.AdoptionApplicationReceivedPlaceholders.TemplateId,
                    Type = NotificationType.InApp,
                    TitleMappings = new Dictionary<String, String>(),
                    ContentMappings = new Dictionary<String, String>
                    {
                        { _notificationConfig.AdoptionApplicationReceivedPlaceholders.UserName, user.FullName },
                        { _notificationConfig.AdoptionApplicationReceivedPlaceholders.ApplicationId, data.Id },
                        { _notificationConfig.AdoptionApplicationReceivedPlaceholders.AnimalName, referedAnimal.Name },
                    },
                };

                generatedEvents.Add(applicationReceivedEvent);
            }
            else
            {
                if (statusChanged)
                {
                    // Inform user that their application has been changed
                    NotificationEvent applicationUpdatedUserEvent = new NotificationEvent()
                    {
                        UserId = data.UserId,
                        TeplateId = _notificationConfig.AdoptionApplicationChangedUserPlaceholders.TemplateId,
                        Type = NotificationType.InApp,
                        TitleMappings = new Dictionary<String, String>(),
                        ContentMappings = new Dictionary<String, String>
                        {
                            { _notificationConfig.AdoptionApplicationChangedUserPlaceholders.UserFirstName, userFirstName },
                            { _notificationConfig.AdoptionApplicationChangedUserPlaceholders.ShelterName, receivingShelter.ShelterName },
                            { _notificationConfig.AdoptionApplicationChangedUserPlaceholders.ApplicationId, data.Id },
                            { _notificationConfig.AdoptionApplicationChangedUserPlaceholders.AnimalName, referedAnimal.Name },
                            { _notificationConfig.AdoptionApplicationChangedUserPlaceholders.ApplicationStatus, data.Status.ToString() },
                        },
                    };

                    generatedEvents.Add(applicationUpdatedUserEvent);
                }

                // Inform shelter that their received application has been changed
                if (changeMadeByUser)
                {
                    // Inform user that their application has been changed
                    NotificationEvent applicationUpdateShelterEvent = new NotificationEvent()
                    {
                        UserId = receivingShelter.UserId,
                        TeplateId = _notificationConfig.AdoptionApplicationChangedShelterPlaceholders.TemplateId,
                        Type = NotificationType.InApp,
                        TitleMappings = new Dictionary<String, String>(),
                        ContentMappings = new Dictionary<String, String>
                        {
                            { _notificationConfig.AdoptionApplicationChangedShelterPlaceholders.ApplicationId, data.Id },
                            { _notificationConfig.AdoptionApplicationChangedShelterPlaceholders.AnimalName, referedAnimal.Name },
                            { _notificationConfig.AdoptionApplicationChangedShelterPlaceholders.UserFullName, user.FullName },
                        },
                    };

                    generatedEvents.Add(applicationUpdateShelterEvent);
                }
            }

            // Send all these generated event notifications to the notification service
            if (generatedEvents.Count > 0) await _notificationApiClient.NotificationEvent(generatedEvents);
        }

        private async Task NotifyRejected(List<Data.Entities.AdoptionApplication> rejectedApplications)
        {
            if (rejectedApplications == null || rejectedApplications.Count == 0) return;

            List<NotificationEvent> events = new List<NotificationEvent>();

            Data.Entities.Shelter shelter = await _shelterRepository.FindAsync(s => s.Id == rejectedApplications.FirstOrDefault().ShelterId, new List<String> { nameof(Data.Entities.Shelter.ShelterName) });
            if (shelter == null) throw new NotFoundException("Shelter to send notification from not found");

            Data.Entities.Animal animal = await _animalRepository.FindAsync(a => a.Id == rejectedApplications.FirstOrDefault().AnimalId, new List<String> { nameof(Data.Entities.Animal.Name) });
            if (animal == null) throw new NotFoundException("Refering animal from rejected applications not found");


            List<String> applicationsUserIds = rejectedApplications.Select(app => app.UserId).Distinct().ToList();
            Dictionary<String, String> userNames = (await _userRepository.FindManyAsync(u => applicationsUserIds.Contains(u.Id), new List<String> { nameof(Data.Entities.User.Id), nameof(Data.Entities.User.FullName) })).ToDictionary(x => x.Id, x => UserDataHelper.GetFirstNameFormatted(x.FullName));

            foreach (Data.Entities.AdoptionApplication app in rejectedApplications)
            {
                String userName = userNames.ContainsKey(app.UserId) ? userNames[app.UserId] : "User";

                // Reuse the "application changed (user)" template; embed the reason in the status text
                NotificationEvent ev = new NotificationEvent
                {
                    UserId = app.UserId,
                    TeplateId = _notificationConfig.AdoptionApplicationChangedUserPlaceholders.TemplateId,
                    Type = NotificationType.InApp,
                    TitleMappings = new Dictionary<String, String>(),
                    ContentMappings = new Dictionary<String, String>
                    {
                        { _notificationConfig.AdoptionApplicationChangedUserPlaceholders.UserFirstName, userName },
                        { _notificationConfig.AdoptionApplicationChangedUserPlaceholders.ShelterName, shelter.ShelterName },
                        { _notificationConfig.AdoptionApplicationChangedUserPlaceholders.ApplicationId, app.Id },
                        { _notificationConfig.AdoptionApplicationChangedUserPlaceholders.AnimalName, animal.Name },
                        { _notificationConfig.AdoptionApplicationChangedUserPlaceholders.ApplicationStatus, "Rejected : the animal has been adopted by another applicant" }
                    }
                };

                events.Add(ev);
            }

            await _notificationApiClient.NotificationEvent(events);
        }

        #endregion

        #region Request Exists
        public async Task<String> AdoptionRequestExists(String animalId)
        {
            ClaimsPrincipal claimsPrincipal = _authorizationContentResolver.CurrentPrincipal();
            String userId = _claimsExtractor.CurrentUserId(claimsPrincipal);
            if (!_conventionService.IsValidId(userId)) throw new ForbiddenException("No authenticated user found");

            return (await _adoptionApplicationRepository.FindAsync(application => application.UserId.Equals(userId) && application.AnimalId.Equals(animalId), new List<String> { nameof(Data.Entities.AdoptionApplication.Id) }))?.Id;
        }
        #endregion

        #region Delete

        public async Task Delete(String id) { await this.Delete(new List<String>() { id }); }

        public async Task Delete(List<String> ids)
        {
            if (ids == null || !ids.Any()) return;

            AdoptionApplicationLookup lookup = new AdoptionApplicationLookup();
            lookup.Ids = ids;
            lookup.Offset = 0;
            lookup.PageSize = 10000;
            lookup.Fields = new List<String> {
                nameof(Models.AdoptionApplication.AdoptionApplication.Id),
                nameof(Models.AdoptionApplication.AdoptionApplication.Shelter) + "." + nameof(Models.Shelter.Shelter.Id),
                nameof(Models.AdoptionApplication.AdoptionApplication.User) + "." + nameof(Models.User.User.Id),
                nameof(Models.AdoptionApplication.AdoptionApplication.AttachedFiles) + "." + nameof(Models.File.File.Id),
                };

            List<Data.Entities.AdoptionApplication> datas = await lookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation).CollectAsync();

            if (datas == null || !datas.Any()) return;

            if (!await this.AuthoriseAdoptionApplication(datas, Permission.DeleteAdoptionApplications))
                throw new ForbiddenException("Unauthorised access", typeof(Data.Entities.AdoptionApplication), Permission.DeleteAdoptionApplications);

            await _fileService.Value.Delete([.. datas.Where(data => data.AttachedFilesIds != null).SelectMany(data => data.AttachedFilesIds)], false);

            await _adoptionApplicationRepository.DeleteManyAsync(ids);
        }

        public async Task DeleteFromAnimal(String animalId) => await this.DeleteFromAnimals(new List<String> { animalId });

        public async Task DeleteFromAnimals(List<String> animalIds)
        {
            if (animalIds == null || !animalIds.Any()) return;

            AdoptionApplicationLookup lookup = new AdoptionApplicationLookup();
            lookup.AnimalIds = animalIds;
            lookup.Offset = 0;
            lookup.PageSize = 10000;
            lookup.Fields = new List<String> {
                nameof(Models.AdoptionApplication.AdoptionApplication.Id),
            };

            List<Data.Entities.AdoptionApplication> datas = await lookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation).CollectAsync();

            if (datas == null || !datas.Any()) return;

            await this.Delete(datas.Select(x => x.Id).ToList());
        }

        public async Task<Boolean> CanDeleteApplication(String applicationId)
        {
            ClaimsPrincipal currentUser = _authorizationContentResolver.CurrentPrincipal();
            String userId = _claimsExtractor.CurrentUserId(currentUser);
            if (!_conventionService.IsValidId(userId)) throw new UnAuthenticatedException("User is not authenticated.");


            AdoptionApplicationLookup adoptionApplicationLookup = new AdoptionApplicationLookup();
            adoptionApplicationLookup.Ids = [applicationId];

            AuthContext context = _contextBuilder.OwnedFrom(adoptionApplicationLookup).AffiliatedWith(adoptionApplicationLookup).Build();

            return await _authorizationService.AuthorizeOrOwnedOrAffiliated(context, Permission.DeleteAdoptionApplications);
        }

        #endregion

    }
}
