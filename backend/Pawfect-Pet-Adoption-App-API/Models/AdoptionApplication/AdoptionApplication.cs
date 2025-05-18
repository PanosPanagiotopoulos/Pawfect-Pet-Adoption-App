using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
using Pawfect_Pet_Adoption_App_API.Models.Animal;
using Pawfect_Pet_Adoption_App_API.Models.File;
using Pawfect_Pet_Adoption_App_API.Models.Shelter;
using Pawfect_Pet_Adoption_App_API.Models.User;

namespace Pawfect_Pet_Adoption_App_API.Models.AdoptionApplication
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
