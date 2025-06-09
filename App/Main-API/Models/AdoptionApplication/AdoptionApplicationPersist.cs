using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
using Pawfect_Pet_Adoption_App_API.Models.File;

namespace Pawfect_Pet_Adoption_App_API.Models.AdoptionApplication
{
	public class AdoptionApplicationPersist
	{
		public String Id { get; set; }
		public String AnimalId { get; set; }
		public String ShelterId { get; set; }
		public ApplicationStatus Status { get; set; }
		public String ApplicationDetails { get; set; }
		public List<String> AttachedFilesIds { get; set; }
	}
}
