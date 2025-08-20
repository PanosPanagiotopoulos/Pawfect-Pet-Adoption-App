using Pawfect_API.Data.Entities.EnumTypes;
using Pawfect_API.Models.AnimalType;
using Pawfect_API.Models.Breed;
using Pawfect_API.Models.File;
using Pawfect_API.Models.Shelter;

namespace Pawfect_API.Models.Animal
{
	public class Animal
	{
		public String? Id { get; set; }
		public String? Name { get; set; }
		public Double? Age { get; set; }
		public Gender? Gender { get; set; }
		public String? Description { get; set; }
		public Double? Weight { get; set; }
		public String? HealthStatus { get; set; }
		public Shelter.Shelter? Shelter { get; set; }
		public Breed.Breed? Breed { get; set; }
		public AnimalType.AnimalType? AnimalType { get; set; }
		public List<File.File>? AttachedPhotos { get; set; }
		public AdoptionStatus? AdoptionStatus { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}
}
