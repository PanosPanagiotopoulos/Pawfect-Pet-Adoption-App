using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;

namespace Pawfect_Pet_Adoption_App_API.Models.Animal
{
    public class AnimalPersist
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double Age { get; set; }
        public Gender Gender { get; set; }
        public string Description { get; set; }
        public double Weight { get; set; }
        public string HealthStatus { get; set; }
        public string ShelterId { get; set; }
        public string BreedId { get; set; }
        public string TypeId { get; set; }
        public string[] Photos { get; set; }
        public AdoptionStatus AdoptionStatus { get; set; } = AdoptionStatus.Pending;
    }
}
