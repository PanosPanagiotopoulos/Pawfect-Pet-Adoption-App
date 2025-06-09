using Pawfect_Pet_Adoption_App_API.Models.Animal;
using Pawfect_Pet_Adoption_App_API.Models.User;

namespace Pawfect_Pet_Adoption_App_API.Models.Conversation
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
