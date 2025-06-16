using Main_API.Data.Entities.EnumTypes;

namespace Main_API.Models.Notification
{
	public class NotificationPersist
	{
		public String Id { get; set; }

		public String UserId { get; set; }

		public NotificationType Type { get; set; }

		public String Content { get; set; }

		public Boolean IsRead { get; set; } = false;
	}
}
