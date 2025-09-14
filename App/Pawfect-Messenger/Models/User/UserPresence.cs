using Pawfect_Messenger.Data.Entities.EnumTypes;

namespace Pawfect_Messenger.Models.User
{
    public class UserPresence
    {
        public String UserId { get; set; }
        public UserStatus Status { get; set; }
    }
}
