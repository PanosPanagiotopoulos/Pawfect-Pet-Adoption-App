using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Notification;
using Pawfect_Pet_Adoption_App_API.Query.Queries;

namespace Pawfect_Pet_Adoption_App_API.Services.NotificationServices
{
	public class NotificationService : INotificationService
	{
		private readonly NotificationQuery _notificationQuery;
		private readonly NotificationBuilder _notificationBuilder;

		public NotificationService(NotificationQuery notificationQuery, NotificationBuilder notificationBuilder)
		{
			_notificationQuery = notificationQuery;
			_notificationBuilder = notificationBuilder;
		}

		public async Task<IEnumerable<NotificationDto>> QueryNotificationsAsync(NotificationLookup notificationLookup)
		{
			List<Notification> queriedNotifications = await notificationLookup.EnrichLookup(_notificationQuery).CollectAsync();
			return await _notificationBuilder.SetLookup(notificationLookup).BuildDto(queriedNotifications, notificationLookup.Fields.ToList());
		}

		public async Task<NotificationDto?> Get(String id, List<String> fields)
		{
			NotificationLookup lookup = new NotificationLookup(_notificationQuery);
			lookup.Ids = new List<String> { id };
			lookup.Fields = fields;
			lookup.PageSize = 1;
			lookup.Offset = 0;

			List<Notification> notification = await lookup.EnrichLookup().CollectAsync();

			if (notification == null)
			{
				throw new InvalidDataException("Δεν βρέθηκε ειδοποίηση με αυτό το ID");
			}

			return (await _notificationBuilder.SetLookup(lookup).BuildDto(notification, fields)).FirstOrDefault();
		}
	}
}