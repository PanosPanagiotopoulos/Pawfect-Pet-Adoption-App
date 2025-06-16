using Main_API.Data.Entities.EnumTypes;
using Main_API.Models.File;

namespace Main_API.Models.AdoptionApplication
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
