using Pawfect_Notifications.Data.Entities.EnumTypes;

namespace Pawfect_Notifications.Models.User
{
	public class User
	{
		public String? Id { get; set; }
		public String Email { get; set; }
		public String FullName { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
    }
}
