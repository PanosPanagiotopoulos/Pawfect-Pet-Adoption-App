using Pawfect_API.Data.Entities.EnumTypes;

namespace Pawfect_API.Models.AdoptionApplication
{
	public class AdoptionApplication
	{
		public String? Id { get; set; }
		public User.User? User { get; set; }
		public Animal.Animal? Animal { get; set; }
		public Shelter.Shelter? Shelter { get; set; }
		public ApplicationStatus? Status { get; set; }
		public String? ApplicationDetails { get; set; }
		public List<File.File>? AttachedFiles { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}
}
