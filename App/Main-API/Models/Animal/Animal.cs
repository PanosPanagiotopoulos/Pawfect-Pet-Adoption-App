using Main_API.Data.Entities.EnumTypes;
using Main_API.Models.AnimalType;
using Main_API.Models.Breed;
using Main_API.Models.File;
using Main_API.Models.Shelter;

namespace Main_API.Models.Animal
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
