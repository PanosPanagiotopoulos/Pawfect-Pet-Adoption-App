using Pawfect_Messenger.Data.Entities.EnumTypes;

namespace Pawfect_Messenger.Models.Conversation
{
    public class ConversationPersist
    {
        public String Id { get; set; }
        public ConversationType? Type { get; set; }

        public List<String> Participants { get; set; }

        public String CreatedBy { get; set; }
    }
}
