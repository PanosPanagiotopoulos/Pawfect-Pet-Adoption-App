using Main_API.Data.Entities.EnumTypes;

namespace Main_API.Models.Animal
{
	public class AnimalPersist
	{
		public String Id { get; set; }
		public String Name { get; set; }
		public double Age { get; set; }
		public Gender Gender { get; set; }
		public String Description { get; set; }
		public double Weight { get; set; }
		public String HealthStatus { get; set; }
		public String BreedId { get; set; }
		public String AnimalTypeId { get; set; }
		public List<String>? AttachedPhotosIds { get; set; }
		public AdoptionStatus AdoptionStatus { get; set; }
	}
}
