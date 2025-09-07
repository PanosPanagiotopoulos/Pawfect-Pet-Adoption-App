using Pawfect_Messenger.Data.Entities.EnumTypes;

namespace Pawfect_Messenger.Models.Conversation
{
	public class Conversation
	{
		public String Id { get; set; }
        public ConversationType Type { get; set; }
        public List<User.User> Participants { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public Message.Message LastMessagePreview { get; set; } 
        public User.User CreatedBy { get; set; } 
        public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}
}
