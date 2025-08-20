using AutoMapper;
using Microsoft.Extensions.Options;
using Pawfect_Notifications.Builders;
using Pawfect_Notifications.Censors;
using Pawfect_Notifications.Data.Entities.Types.Authorization;
using Pawfect_Notifications.Exceptions;
using Pawfect_Notifications.Models.Lookups;
using Pawfect_Notifications.Models.Notification;
using Pawfect_Notifications.Query;
using Pawfect_Notifications.Repositories.Interfaces;
using Pawfect_Notifications.Services.AuthenticationServices;
using Pawfect_Notifications.Services.Convention;
using Pawfect_Notifications.Data.Entities.EnumTypes;
using Pawfect_Notifications.Data.Entities.Types.Notifications;

namespace Pawfect_Notifications.Services.NotificationServices
{
	public class NotificationService : INotificationService
	{
        private readonly IQueryFactory _queryFactory;
        private readonly IBuilderFactory _builderFactory;
        private readonly INotificationRepository _notificationRepository;
        private readonly ICensorFactory _censorFactory;
        private readonly IAuthorizationService _authorizationService;
        private readonly IConventionService _conventionService;
        private readonly NotificationConfig _config;

        public NotificationService
		(
			IQueryFactory queryFactory,
            IBuilderFactory builderFactory,
            INotificationRepository notificationRepository,
			ICensorFactory censorFactory,
			IAuthorizationService authorizationService,
            IConventionService conventionService,
			IOptions<NotificationConfig> options
		)
		{
            _queryFactory = queryFactory;
            _builderFactory = builderFactory;
            _notificationRepository = notificationRepository;
            _censorFactory = censorFactory;
            _authorizationService = authorizationService;
            _conventionService = conventionService;
            _config = options.Value;
        }

		public async Task<Models.Notification.Notification> Persist(NotificationEvent persist, List<String> fields)
		{
            Data.Entities.Notification data = new Data.Entities.Notification()
			{
				Id = null,
				UserId = persist.UserId,
				Type = persist.Type,
				Status = NotificationStatus.Pending,
				Title = null,
				Content = null,
                TitleMappings = persist.TitleMappings,
                ContentMappings = persist.ContentMappings,
				TeplateId = persist.TeplateId.Value,
				IsRead = false,
				RetryCount = 0,
				MaxRetries = _config.MaxRetries,
				ProcessedAt = null,
				CreatedAt = DateTime.UtcNow,
			};


			String dataId = await _notificationRepository.AddAsync(data);

			if (String.IsNullOrEmpty(dataId))
				throw new InvalidOperationException("Failed to persist notification");

			// Return dto model
			NotificationLookup lookup = new NotificationLookup();
			lookup.Ids = new List<String> { dataId };
			lookup.Fields = fields;
			lookup.Offset = 0;
			lookup.PageSize = 1;
            

            return (await _builderFactory.Builder<NotificationBuilder>().Authorise(AuthorizationFlags.OwnerOrPermission)
					.Build(await lookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermission).CollectAsync(), fields))
					.FirstOrDefault();
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