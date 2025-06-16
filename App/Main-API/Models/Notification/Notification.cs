using Main_API.Data.Entities.EnumTypes;
using Main_API.Models.User;

namespace Main_API.Models.Notification
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
