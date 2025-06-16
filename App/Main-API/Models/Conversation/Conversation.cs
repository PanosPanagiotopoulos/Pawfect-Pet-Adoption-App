using Main_API.Models.Animal;
using Main_API.Models.User;

namespace Main_API.Models.Conversation
{
	public class Conversation
	{
		public String? Id { get; set; }
		public List<User.User>? Users { get; set; }
		public Animal.Animal? Animal { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}
}
