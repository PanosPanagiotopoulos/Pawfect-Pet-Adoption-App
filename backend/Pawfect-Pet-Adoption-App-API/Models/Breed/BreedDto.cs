using Pawfect_Pet_Adoption_App_API.Models.AnimalType;

namespace Pawfect_Pet_Adoption_App_API.Models.Breed
{
    public class BreedDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string TypeId { get; set; }
        public AnimalTypeDto? AnimalType { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
