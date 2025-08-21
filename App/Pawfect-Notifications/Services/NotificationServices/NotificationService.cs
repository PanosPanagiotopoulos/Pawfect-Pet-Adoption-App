using Microsoft.Extensions.Options;
using Pawfect_Notifications.Data.Entities.Types.Authorization;
using Pawfect_Notifications.Exceptions;
using Pawfect_Notifications.Models.Notification;
using Pawfect_Notifications.Repositories.Interfaces;
using Pawfect_Notifications.Services.AuthenticationServices;
using Pawfect_Notifications.Data.Entities.EnumTypes;
using Pawfect_Notifications.Data.Entities.Types.Notifications;

namespace Pawfect_Notifications.Services.NotificationServices
{
	public class NotificationService : INotificationService
	{
        private readonly ILogger<NotificationService> _logger;
        private readonly INotificationRepository _notificationRepository;
        private readonly IAuthorizationService _authorizationService;
        private readonly NotificationConfig _config;

        public NotificationService
        (
            ILogger<NotificationService> logger,
            INotificationRepository notificationRepository,
			IAuthorizationService authorizationService,
			IOptions<NotificationConfig> options
		)
		{
            _logger = logger;
            _notificationRepository = notificationRepository;
            _authorizationService = authorizationService;
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

        public async Task Delete(String id) { await this.Delete(new List<String>() { id }); }

		public async Task Delete(List<String> ids)
		{
			if (!await _authorizationService.AuthorizeAsync(Permission.DeleteNotifications))
                throw new ForbiddenException("Unauthorised access when deleting notifications", typeof(Data.Entities.Notification), Permission.DeleteNotifications);

            await _notificationRepository.DeleteManyAsync(ids);
		}
	}
}