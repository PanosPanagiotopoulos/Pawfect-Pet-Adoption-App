namespace Pawfect_API.Data.Entities.Types.AiContext
{
    public class AnimalContext
    {
        // Is Focused Document
        public Boolean IsFocusedDocument { get; set; }
        public String Id { get; set; }
        public String Name { get; set; }
        public Double Age { get; set; }
        public String Gender { get; set; }
        public String HealthStatus { get; set; }
        public String Description { get; set; }
        public Double Weight { get; set; }
        public List<String> PhotoUrls { get; set; }

        // AnimalType
        public String AnimalTypeName { get; set; }
        public String AnimalTypeDescription { get; set; }

        // Breed
        public String BreedName { get; set; }
        public String BreedDescription { get; set; }

        // Shelter
        public String ShelterId { get; set; }
        public String ShelterName { get; set; }


        public static AnimalContext FromAnimalModel(Models.Animal.Animal animal) => FromAnimalModels([animal], [animal.Id]).FirstOrDefault();

        /// <summary>
        /// Convert a list of Animal models into simplified AnimalContext models.
        /// </summary>
        public static List<AnimalContext> FromAnimalModels(List<Models.Animal.Animal> animals, List<String> focusedDocuments = null)
        {
            if (animals == null || animals.Count == 0) return new List<AnimalContext>();

            HashSet<String> focusedDocumentsSet = focusedDocuments?.ToHashSet() ?? new HashSet<String>();

            return animals.Select(a => new AnimalContext
            {
                IsFocusedDocument = focusedDocumentsSet.Contains(a.Id),
                Id = a.Id,
                Name = a.Name,
                Age = a.Age ?? 1,
                Gender = a.Gender.ToString(),
                HealthStatus = a.HealthStatus,
                Description = a.Description,
                Weight = a.Weight ?? 5,
                PhotoUrls = a.AttachedPhotos?.Select(p => p.SourceUrl).ToList(),
                AnimalTypeName = a.AnimalType?.Name,
                AnimalTypeDescription = a.AnimalType?.Description,
                BreedName = a.Breed?.Name,
                BreedDescription = a.Breed?.Description,
                ShelterId = a.Shelter?.Id,
                ShelterName = a.Shelter?.ShelterName
            }).ToList();
        }
    }
}
