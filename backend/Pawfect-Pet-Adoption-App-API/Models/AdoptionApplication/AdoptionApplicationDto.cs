using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
using Pawfect_Pet_Adoption_App_API.Models.Animal;
using Pawfect_Pet_Adoption_App_API.Models.Shelter;
using Pawfect_Pet_Adoption_App_API.Models.User;

namespace Pawfect_Pet_Adoption_App_API.Models.AdoptionApplication
{
	public class AdoptionApplicationDto
	{
		public String? Id { get; set; }
		public UserDto? User { get; set; }
		public AnimalDto? Animal { get; set; }
		public ShelterDto? Shelter { get; set; }
		public ApplicationStatus? Status { get; set; }
		public String? ApplicationDetails { get; set; }
		public List<String>? AttachedFilesUrls { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}
}
