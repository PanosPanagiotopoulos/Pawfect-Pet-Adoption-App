using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
using Pawfect_Pet_Adoption_App_API.Models.AnimalType;
using Pawfect_Pet_Adoption_App_API.Models.Breed;
using Pawfect_Pet_Adoption_App_API.Models.Shelter;

namespace Pawfect_Pet_Adoption_App_API.Models.Animal
{
	public class AnimalDto
	{
		public String? Id { get; set; }
		public String? Name { get; set; }
		public double? Age { get; set; }
		public Gender? Gender { get; set; }
		public String? Description { get; set; }
		public double? Weight { get; set; }
		public String? HealthStatus { get; set; }
		public ShelterDto? Shelter { get; set; }
		public BreedDto? Breed { get; set; }
		public AnimalTypeDto? AnimalType { get; set; }
		public List<String>? Photos { get; set; }
		public AdoptionStatus? AdoptionStatus { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}
}
