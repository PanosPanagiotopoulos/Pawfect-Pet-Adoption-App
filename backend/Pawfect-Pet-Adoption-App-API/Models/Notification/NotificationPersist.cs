using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;

namespace Pawfect_Pet_Adoption_App_API.Models.Notification
{
    public class NotificationPersist
    {
        public string Id { get; set; }

        public string UserId { get; set; }

        public NotificationType Type { get; set; }

        public string Content { get; set; }

        public bool IsRead { get; set; } = false;
    }
}
