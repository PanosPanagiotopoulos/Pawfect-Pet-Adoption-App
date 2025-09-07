using Pawfect_Messenger.Data.Entities.EnumTypes;

namespace Pawfect_Messenger.Models.Message
{
	public class Message
	{
		public String Id { get; set; }
		public Conversation.Conversation? Conversation { get; set; }
		public User.User Sender { get; set; }
        public List<User.User> ReadBy { get; set; }
        public MessageType? Type { get; set; }
		public String Content { get; set; }
        public MessageStatus? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
