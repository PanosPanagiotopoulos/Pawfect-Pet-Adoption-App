using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;

namespace Pawfect_Pet_Adoption_App_API.Models.AdoptionApplication
{
	public class AdoptionApplicationPersist
	{
		public String Id { get; set; }
		public String UserId { get; set; }
		public String AnimalId { get; set; }
		public String ShelterId { get; set; }
		public ApplicationStatus Status { get; set; }
		public String ApplicationDetails { get; set; }

		// *TODO* Set how the saving , parsing and validation of form file data will be done
		public List<IFormFile> AttachedFiles { get; set; }

	}
}
