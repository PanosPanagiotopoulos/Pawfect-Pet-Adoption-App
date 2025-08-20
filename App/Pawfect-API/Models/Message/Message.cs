using Pawfect_API.Models.Conversation;
using Pawfect_API.Models.User;

namespace Pawfect_API.Models.Message
{
	public class Message
	{
		public String? Id { get; set; }
		public Conversation.Conversation? Conversation { get; set; }
		public User.User? Sender { get; set; }
		public User.User? Recipient { get; set; }
		public String? Content { get; set; }
		public Boolean? IsRead { get; set; }
		public DateTime? CreatedAt { get; set; }
	}
}
