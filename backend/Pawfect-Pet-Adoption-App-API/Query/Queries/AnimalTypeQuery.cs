// FILE: Query/Queries/AnimalTypeQuery.cs
using MongoDB.Driver;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Models.AnimalType;
using Pawfect_Pet_Adoption_App_API.Services;

namespace Pawfect_Pet_Adoption_App_API.Query.Queries
{
    public class AnimalTypeQuery : BaseQuery<AnimalType>
    {
        // Κατασκευαστής για την κλάση AnimalTypeQuery
        // Είσοδος: mongoDbService - μια έκδοση της κλάσης MongoDbService
        public AnimalTypeQuery(MongoDbService mongoDbService)
        {
            base._collection = mongoDbService.GetCollection<AnimalType>();
        }

        // Λίστα από IDs τύπων ζώων για φιλτράρισμα
        public List<string>? Ids { get; set; }

        // Ονομασία τύπων ζώων για φιλτράρισμα
        public string? Name { get; set; }

        // Εφαρμόζει τα καθορισμένα φίλτρα στο ερώτημα
        // Έξοδος: FilterDefinition<AnimalType> - ο ορισμός φίλτρου που θα χρησιμοποιηθεί στο ερώτημα
        protected override Task<FilterDefinition<AnimalType>> ApplyFilters()
        {
            FilterDefinitionBuilder<AnimalType> builder = Builders<AnimalType>.Filter;
            FilterDefinition<AnimalType> filter = builder.Empty;

            // Εφαρμόζει φίλτρο για τα IDs των τύπων ζώων
            if (Ids != null && Ids.Any())
            {
                filter &= builder.In(animalType => animalType.Id, Ids);
            }

            // Εφαρμόζει φίλτρο για την ονομασία των τύπων ζώων
            if (!string.IsNullOrEmpty(Name))
            {
                filter &= builder.Eq(animalType => animalType.Name, Name);
            }

            // Εφαρμόζει φίλτρο για fuzzy search μέσω indexing στη Mongo σε πεδίο : Name
            if (!string.IsNullOrEmpty(Query))
            {
                filter &= builder.Text(Query);
            }

            return Task.FromResult(filter);
        }

        // Επιστρέφει τα ονόματα πεδίων που θα προβληθούν στο αποτέλεσμα του ερωτήματος
        // Είσοδος: fields - μια λίστα με τα ονόματα των πεδίων που θα προβληθούν
        // Έξοδος: List<string> - τα ονόματα των πεδίων που θα προβληθούν
        public override List<string> FieldNamesOf(List<string> fields)
        {
            if (fields == null) return new List<string>();
            if (fields.Any() || fields.Contains("*")) return EntityHelper.GetAllPropertyNames(typeof(AnimalType)).ToList();

            HashSet<string> projectionFields = new HashSet<string>();
            foreach (string item in fields)
            {
                // Αντιστοιχίζει τα ονόματα πεδίων AnimalTypeDto στα ονόματα πεδίων AnimalType
                if (item.Equals(nameof(AnimalTypeDto.Id))) projectionFields.Add(nameof(AnimalType.Id));
                if (item.Equals(nameof(AnimalTypeDto.Name))) projectionFields.Add(nameof(AnimalType.Name));
                if (item.Equals(nameof(AnimalTypeDto.Description))) projectionFields.Add(nameof(AnimalType.Description));
                if (item.Equals(nameof(AnimalTypeDto.CreatedAt))) projectionFields.Add(nameof(AnimalType.CreatedAt));
                if (item.Equals(nameof(AnimalTypeDto.UpdatedAt))) projectionFields.Add(nameof(AnimalType.UpdatedAt));
            }

            return projectionFields.ToList();
        }
    }
}