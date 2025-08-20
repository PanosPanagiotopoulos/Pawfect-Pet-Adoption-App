using Pawfect_API.Models.AnimalType;

namespace Pawfect_API.Models.Breed
{
	public class Breed
	{
		public String? Id { get; set; }
		public String? Name { get; set; }
		public AnimalType.AnimalType? AnimalType { get; set; }
		public String? Description { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}
}
