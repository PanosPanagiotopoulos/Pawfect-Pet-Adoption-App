using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Notification;

namespace Pawfect_Pet_Adoption_App_API.Services.NotificationServices
{
	public interface INotificationService
	{
		// Συνάρτηση για query στα notifications
		Task<IEnumerable<NotificationDto>> QueryNotificationsAsync(NotificationLookup notificationLookup);

		Task<NotificationDto?> Get(String id, List<String> fields);

		Task<NotificationDto?> Persist(NotificationPersist persist);

	}
}