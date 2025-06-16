using Main_API.Models.Lookups;
using Main_API.Models.Notification;

namespace Main_API.Services.NotificationServices
{
	public interface INotificationService
	{
		Task<Notification?> Persist(NotificationPersist persist, List<String> fields);
		Task Delete(String id);
		Task Delete(List<String> ids);

	}
}