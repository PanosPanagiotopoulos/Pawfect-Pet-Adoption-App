using Pawfect_Messenger.Data.Entities.EnumTypes;

namespace Pawfect_Messenger.Models.Message
{
    public class MessagePersist
    {
        public String Id { get; set; }
        public String ConversationId { get; set; }
        public String SenderId { get; set; }
        public MessageType? Type { get; set; }
        public String Content { get; set; }
    }

    public class MessageReadPersist
    {
        public String MessageId { get; set; }
        public List<String> UserIds { get; set; }
    }
}
