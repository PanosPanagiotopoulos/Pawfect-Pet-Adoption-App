using Pawfect_Notifications.Data.Entities.EnumTypes;
using Pawfect_Notifications.Models.User;

namespace Pawfect_Notifications.Models.Notification
{
	public class Notification
	{
		public String? Id { get; set; }
		public User.User? User { get; set; }
		public NotificationType? Type { get; set; }
		public String? Content { get; set; }
		public Boolean? IsRead { get; set; }
		public DateTime? CreatedAt { get; set; }
	}
}
