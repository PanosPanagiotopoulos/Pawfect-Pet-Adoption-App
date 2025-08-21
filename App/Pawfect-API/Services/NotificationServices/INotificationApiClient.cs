using Pawfect_API.Models.Lookups;
using Pawfect_API.Models.Notification;

namespace Pawfect_API.Services.NotificationServices
{
	public interface INotificationApiClient
	{
		Task NotificationEvent(NotificationEvent notificationEvent);
        Task NotificationEvent(List<NotificationEvent> notificationEvents);
    }
}