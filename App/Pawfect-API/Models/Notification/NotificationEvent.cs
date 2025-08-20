using Pawfect_API.Data.Entities.EnumTypes;

namespace Pawfect_API.Models.Notification
{
	public class NotificationEvent
	{
        public String UserId { get; set; }
        public NotificationType Type { get; set; }
        public Guid? TeplateId { get; set; }
        public Dictionary<String, String> TitleMappings { get; set; }
        public Dictionary<String, String> ContentMappings { get; set; }
    }
}
