using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Notification;

namespace Pawfect_Pet_Adoption_App_API.Services.NotificationServices
{
	public interface INotificationService
	{
		Task<NotificationDto?> Persist(NotificationPersist persist, List<String> fields);
		Task Delete(String id);
		Task Delete(List<String> ids);

	}
}