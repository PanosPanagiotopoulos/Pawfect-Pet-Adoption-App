using Pawfect_Pet_Adoption_App_API.Models.Conversation;
using Pawfect_Pet_Adoption_App_API.Models.User;

namespace Pawfect_Pet_Adoption_App_API.Models.Message
{
    public class MessageDto
    {
        public string Id { get; set; }
        public ConversationDto? Conversation { get; set; }
        public UserDto? Sender { get; set; }
        public UserDto? Recipient { get; set; }
        public string Content { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
