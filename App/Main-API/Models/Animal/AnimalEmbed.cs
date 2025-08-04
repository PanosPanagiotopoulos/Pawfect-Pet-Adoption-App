namespace Pawfect_Pet_Adoption_App_API.Models.Animal
{
    public class AnimalEmbed
    {
        public String Age { get; set; }
        public String Gender { get; set; }
        public String Description { get; set; }
        public String Weight { get; set; }
        public String HealthStatus { get; set; }

        public String BreedName { get; set; }

        public String BreedDescription { get; set; }

        public String AnimalTypeName { get; set; }

        public String AnimalTypeDescription { get; set; }

        public String ToEmbeddingText() => 
            $"Age: {Age} years old. Gender: {Gender}. " +
            $"Description: {Description}. Weight: {Weight} kg. " +
            $"Health Status: {HealthStatus}. Breed: {BreedName} . " +
            $"Breed Description : {BreedDescription}" +
            $"Animal Type : {AnimalTypeName} . Animal Type Description: {AnimalTypeDescription}"
            .ToLower()
            .Trim();
    }
}
