using Pawfect_Notifications.Models.Lookups;
using Pawfect_Notifications.Models.Notification;

namespace Pawfect_Notifications.Services.NotificationServices
{
	public interface INotificationService
	{
		Task HandleEvent(NotificationEvent @event);
        Task HandleEvents(List<NotificationEvent> events);
		Task<Models.Notification.Notification> ReadNotificationsAsync(String id, List<String> fields = null);
		Task<List<Models.Notification.Notification>> ReadNotificationsAsync(List<String> ids, List<String> fields = null);
        Task Delete(String id);
		Task Delete(List<String> ids);

	}
}