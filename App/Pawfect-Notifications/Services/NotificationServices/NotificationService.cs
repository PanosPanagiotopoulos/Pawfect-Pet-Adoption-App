using Microsoft.Extensions.Options;
using Pawfect_Notifications.Data.Entities.Types.Authorization;
using Pawfect_Notifications.Exceptions;
using Pawfect_Notifications.Models.Notification;
using Pawfect_Notifications.Repositories.Interfaces;
using Pawfect_Notifications.Services.AuthenticationServices;
using Pawfect_Notifications.Data.Entities.EnumTypes;
using Pawfect_Notifications.Data.Entities.Types.Notifications;
using Pawfect_Notifications.Models.Lookups;
using System.Security.Claims;
using MongoDB.Bson.Serialization.Conventions;
using Pawfect_Notifications.Services.Convention;
using Pawfect_Notifications.DevTools;
using Pawfect_Notifications.Censors;
using Pawfect_Notifications.Builders;
using Pawfect_Notifications.Query;
using SendGrid.Helpers.Mail;

namespace Pawfect_Notifications.Services.NotificationServices
{
	public class NotificationService : INotificationService
	{
        private readonly ILogger<NotificationService> _logger;
        private readonly INotificationRepository _notificationRepository;
        private readonly IAuthorizationService _authorizationService;
        private readonly IAuthorizationContentResolver _authorizationContentResolver;
        private readonly ClaimsExtractor _claimsExtractor;
        private readonly IConventionService _conventionService;
        private readonly IQueryFactory _queryFactory;
        private readonly IBuilderFactory _builderFactory;
        private readonly ICensorFactory _censorFactory;
        private readonly AuthContextBuilder _contextBuilder;
        private readonly NotificationConfig _config;

        public NotificationService
        (
            ILogger<NotificationService> logger,
            INotificationRepository notificationRepository,
			IAuthorizationService authorizationService,
            IAuthorizationContentResolver authorizationContentResolver,
            ClaimsExtractor claimsExtractor,
            IConventionService conventionService,
            IQueryFactory queryFactory,
            IBuilderFactory builderFactory,
            ICensorFactory censorFactory,
            AuthContextBuilder contextBuilder,
            IOptions<NotificationConfig> options
		)
		{
            _logger = logger;
            _notificationRepository = notificationRepository;
            _authorizationService = authorizationService;
            _authorizationContentResolver = authorizationContentResolver;
            _claimsExtractor = claimsExtractor;
            _conventionService = conventionService;
            _queryFactory = queryFactory;
            _builderFactory = builderFactory;
            _censorFactory = censorFactory;
            _contextBuilder = contextBuilder;
            _config = options.Value;
        }

        public async Task HandleEvent(NotificationEvent @event) => await this.HandleEvents([@event]);

        public async Task HandleEvents(List<NotificationEvent> events)
        {
            if (events == null || !events.Any()) throw new ArgumentException("Events list cannot be null or empty");

            List<Data.Entities.Notification> handledNotifications = events.Select(@event => new Data.Entities.Notification()
            {
                Id = null,
                UserId = @event.UserId,
                Type = @event.Type,
                Status = NotificationStatus.Pending,
                Title = null,
                Content = null,
                TitleMappings = @event.TitleMappings,
                ContentMappings = @event.ContentMappings,
                TeplateId = @event.TeplateId.Value,
                IsRead = false,
                RetryCount = 0,
                MaxRetries = _config.MaxRetries,
                ProcessedAt = null,
                CreatedAt = DateTime.UtcNow,
            }).ToList();


            List<String> dataIds = await _notificationRepository.AddManyAsync(handledNotifications);
            if (dataIds == null || dataIds.Count != events.Count)
                _logger.LogError("Failed to persist all notification events. Expected {ExpectedCount}, but got {ActualCount}", events.Count, dataIds?.Count ?? 0);
        }

        public async Task<Models.Notification.Notification> ReadNotificationsAsync(String id, List<String> fields = null) => (await this.ReadNotificationsAsync([id], fields))?.FirstOrDefault();
        public async Task<List<Models.Notification.Notification>> ReadNotificationsAsync(List<String> ids, List<String> fields = null)
        {
            if (ids == null || !ids.Any())
                throw new ArgumentException("Notification IDs cannot be null or empty", nameof(ids));

            ClaimsPrincipal currentUser = _authorizationContentResolver.CurrentPrincipal();
            String userId = _claimsExtractor.CurrentUserId(currentUser);
            if (!_conventionService.IsValidId(userId)) throw new UnAuthenticatedException("User is not authenticated.");

            NotificationLookup authFilter = new NotificationLookup();
            authFilter.UserIds = [userId];
            OwnedResource ownedResource = new OwnedResource(userId, new OwnedFilterParams(authFilter));

            if (!await _authorizationService.AuthorizeOrOwnedAsync(ownedResource, Permission.EditNotifications))
                throw new ForbiddenException();

            List<Data.Entities.Notification> notifications = await _notificationRepository.FindManyAsync(notf => ids.Contains(notf.Id));
            if (notifications == null || !notifications.Any())
                throw new NotFoundException("Notifications not found", JsonHelper.SerializeObjectFormatted(ids), typeof(Data.Entities.Notification));

            notifications.ForEach(notf => { notf.IsRead = true; notf.ReadAt = DateTime.UtcNow; });

            List<String> updatedNotifications = await _notificationRepository.UpdateManyAsync(notifications);

            if (updatedNotifications == null || updatedNotifications.Count != notifications.Count)
                throw new Exception("Failed to update all notifications");

            // Return dto model
            NotificationLookup lookup = new NotificationLookup();
            lookup.Ids = updatedNotifications;
            lookup.Fields = fields;
            lookup.Offset = 0;
            lookup.PageSize = updatedNotifications.Count;

            AuthContext censorContext = _contextBuilder.OwnedFrom(lookup).AffiliatedWith(lookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<NotificationCensor>().Censor([.. lookup.Fields], censorContext);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying adoption applications");

            lookup.Fields = censoredFields;

            return await _builderFactory.Builder<NotificationBuilder>().Authorise(AuthorizationFlags.OwnerOrPermission).Build(
                notifications,
                censoredFields
            );
        }

        public async Task Delete(String id) { await this.Delete(new List<String>() { id }); }

		public async Task Delete(List<String> ids)
		{
			if (!await _authorizationService.AuthorizeAsync(Permission.DeleteNotifications))
                throw new ForbiddenException("Unauthorised access when deleting notifications", typeof(Data.Entities.Notification), Permission.DeleteNotifications);

            await _notificationRepository.DeleteManyAsync(ids);
		}
	}
}