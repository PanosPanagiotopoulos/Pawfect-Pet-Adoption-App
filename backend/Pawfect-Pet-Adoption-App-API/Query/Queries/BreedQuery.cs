using MongoDB.Driver;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Models.Breed;
using Pawfect_Pet_Adoption_App_API.Services;

namespace Pawfect_Pet_Adoption_App_API.Query.Queries
{
    public class BreedQuery : BaseQuery<Breed>
    {
        // Constructor for the BreedQuery class
        // Input: mongoDbService - μια έκδοση της κλάσης MongoDbService
        public BreedQuery(MongoDbService mongoDbService)
        {
            base._collection = mongoDbService.GetCollection<Breed>();
        }

        // Λίστα με τα αναγνωριστικά των φυλών για φιλτράρισμα
        public List<string>? Ids { get; set; }

        // Λίστα με τα αναγνωριστικά των τύπων για φιλτράρισμα
        public List<string>? TypeIds { get; set; }

        // Ημερομηνία έναρξης για φιλτράρισμα (δημιουργήθηκε από)
        public DateTime? CreatedFrom { get; set; }

        // Ημερομηνία λήξης για φιλτράρισμα (δημιουργήθηκε μέχρι)
        public DateTime? CreatedTill { get; set; }

        // Εφαρμόζει τα καθορισμένα φίλτρα στο ερώτημα
        // Έξοδος: FilterDefinition<Breed> - ο ορισμός φίλτρου που θα χρησιμοποιηθεί στο ερώτημα
        protected override Task<FilterDefinition<Breed>> ApplyFilters()
        {
            FilterDefinitionBuilder<Breed> builder = Builders<Breed>.Filter;
            FilterDefinition<Breed> filter = builder.Empty;

            // Εφαρμόζει φίλτρο για τα αναγνωριστικά των φυλών
            if (Ids != null && Ids.Any())
            {
                filter &= builder.In(breed => breed.Id, Ids);
            }

            // Εφαρμόζει φίλτρο για τα αναγνωριστικά των τύπων
            if (TypeIds != null && TypeIds.Any())
            {
                filter &= builder.In(breed => breed.TypeId, TypeIds);
            }

            // Εφαρμόζει φίλτρο για την ημερομηνία έναρξης
            if (CreatedFrom.HasValue)
            {
                filter &= builder.Gte(breed => breed.CreatedAt, CreatedFrom.Value);
            }

            // Εφαρμόζει φίλτρο για την ημερομηνία λήξης
            if (CreatedTill.HasValue)
            {
                filter &= builder.Lte(breed => breed.CreatedAt, CreatedTill.Value);
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
            if (fields.Any() || fields.Contains("*")) return EntityHelper.GetAllPropertyNames(typeof(Breed)).ToList();

            HashSet<string> projectionFields = new HashSet<string>();
            foreach (string item in fields)
            {
                if (item.Equals(nameof(BreedDto.Id))) projectionFields.Add(nameof(Breed.Id));
                if (item.Equals(nameof(BreedDto.Name))) projectionFields.Add(nameof(Breed.Name));
                if (item.Equals(nameof(BreedDto.Description))) projectionFields.Add(nameof(Breed.Description));
                if (item.Equals(nameof(BreedDto.CreatedAt))) projectionFields.Add(nameof(Breed.CreatedAt));
                if (item.Equals(nameof(BreedDto.UpdatedAt))) projectionFields.Add(nameof(Breed.UpdatedAt));
                if (item.StartsWith(nameof(BreedDto.AnimalType))) projectionFields.Add(nameof(Breed.TypeId));
            }
            return projectionFields.ToList();
        }
    }
}