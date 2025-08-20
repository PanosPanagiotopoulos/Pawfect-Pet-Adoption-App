using Pawfect_Notifications.Models.Lookups;
using Pawfect_Notifications.Models.Notification;

namespace Pawfect_Notifications.Services.NotificationServices
{
	public interface INotificationService
	{
		Task<Notification?> Persist(NotificationEvent persist, List<String> fields);
		Task Delete(String id);
		Task Delete(List<String> ids);

	}
}