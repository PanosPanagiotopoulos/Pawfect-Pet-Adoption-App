using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
using Pawfect_Pet_Adoption_App_API.Models.AnimalType;
using Pawfect_Pet_Adoption_App_API.Models.Breed;
using Pawfect_Pet_Adoption_App_API.Models.File;
using Pawfect_Pet_Adoption_App_API.Models.Shelter;

namespace Pawfect_Pet_Adoption_App_API.Models.Animal
{
	public class Animal
	{
		public String? Id { get; set; }
		public String? Name { get; set; }
		public double? Age { get; set; }
		public Gender? Gender { get; set; }
		public String? Description { get; set; }
		public double? Weight { get; set; }
		public String? HealthStatus { get; set; }
		public Shelter.Shelter? Shelter { get; set; }
		public Breed.Breed? Breed { get; set; }
		public AnimalType.AnimalType? AnimalType { get; set; }
		public List<File.File>? Photos { get; set; }
		public AdoptionStatus? AdoptionStatus { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}
}
