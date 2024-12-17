// FILE: Query/Queries/AnimalQuery.cs
using MongoDB.Driver;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Models;
using Pawfect_Pet_Adoption_App_API.Models.Animal;
using Pawfect_Pet_Adoption_App_API.Services;

namespace Pawfect_Pet_Adoption_App_API.Query.Queries
{
    public class AnimalQuery : BaseQuery<Animal>
    {
        private readonly SearchService _searchService;

        // Κατασκευαστής για την κλάση AnimalQuery
        // Είσοδος: mongoDbService - μια έκδοση της κλάσης MongoDbService
        public AnimalQuery(MongoDbService mongoDbService, SearchService searchService)
        {
            base._collection = mongoDbService.GetCollection<Animal>();
            this._searchService = searchService;
        }

        // Λίστα από IDs ζώων για φιλτράρισμα
        public List<string>? Ids { get; set; }

        // Λίστα από IDs καταφυγίων για φιλτράρισμα
        public List<string>? ShelterIds { get; set; }

        // Λίστα από IDs φυλών για φιλτράρισμα
        public List<string>? BreedIds { get; set; }

        // Λίστα από IDs τύπων για φιλτράρισμα
        public List<string>? TypeIds { get; set; }

        // Λίστα από καταστάσεις υιοθεσίας για φιλτράρισμα
        public List<AdoptionStatus>? AdoptionStatuses { get; set; }

        // Ημερομηνία έναρξης για φιλτράρισμα (δημιουργήθηκε από)
        public DateTime? CreateFrom { get; set; }

        // Ημερομηνία λήξης για φιλτράρισμα (δημιουργήθηκε μέχρι)
        public DateTime? CreatedTill { get; set; }

        // Εφαρμόζει τα καθορισμένα φίλτρα στο ερώτημα
        // Έξοδος: FilterDefinition<Animal> - ο ορισμός φίλτρου που θα χρησιμοποιηθεί στο ερώτημα
        protected override async Task<FilterDefinition<Animal>> ApplyFilters()
        {
            FilterDefinitionBuilder<Animal> builder = Builders<Animal>.Filter;
            FilterDefinition<Animal> filter = builder.Empty;

            // Εφαρμόζει φίλτρο για τα IDs των ζώων
            if (Ids != null && Ids.Any())
            {
                filter &= builder.In(animal => animal.Id, Ids);
            }

            // Εφαρμόζει φίλτρο για τα IDs των καταφυγίων
            if (ShelterIds != null && ShelterIds.Any())
            {
                filter &= builder.In(animal => animal.ShelterId, ShelterIds);
            }

            // Εφαρμόζει φίλτρο για τα IDs των φυλών
            if (BreedIds != null && BreedIds.Any())
            {
                filter &= builder.In(animal => animal.BreedId, BreedIds);
            }

            // Εφαρμόζει φίλτρο για τα IDs των τύπων
            if (TypeIds != null && TypeIds.Any())
            {
                filter &= builder.In(animal => animal.TypeId, TypeIds);
            }

            // Εφαρμόζει φίλτρο για τις καταστάσεις υιοθεσίας
            if (AdoptionStatuses != null && AdoptionStatuses.Any())
            {
                filter &= builder.In(animal => animal.AdoptionStatus, AdoptionStatuses);
            }

            // Εφαρμόζει φίλτρο για την ημερομηνία έναρξης
            if (CreateFrom.HasValue)
            {
                filter &= builder.Gte(animal => animal.CreatedAt, CreateFrom.Value);
            }

            // Εφαρμόζει φίλτρο για την ημερομηνία λήξης
            if (CreatedTill.HasValue)
            {
                filter &= builder.Lte(animal => animal.CreatedAt, CreatedTill.Value);
            }

            // Εφαρμόζει φίλτρο για complex search. Εδώ θα επικοινωνεί με τον Search Server για την AI-based αναζήτηση
            if (!string.IsNullOrEmpty(Query))
            {
                // Χρήση υπηρεσίας search για να βρούμε τα ids των ζώων που ταιριάζουν στο query
                SearchRequest searchRequest = new SearchRequest { Query = Query, TopK = base.PageSize, Lang = "el" };
                List<string>? ids = ((await _searchService.SearchAnimalsAsync(searchRequest)) ?? new List<AnimalIndexModel>())
                                    .Select(searchRes => searchRes.Id).ToList();
                // Φιλτράρουμε με αυτά τα ids
                if (ids != null && ids.Any())
                {
                    filter &= builder.In(animal => animal.Id, ids);
                }
            }


            return filter;
        }

        // Επιστρέφει τα ονόματα πεδίων που θα προβληθούν στο αποτέλεσμα του ερωτήματος
        // Είσοδος: fields - μια λίστα με τα ονόματα των πεδίων που θα προβληθούν
        // Έξοδος: List<string> - τα ονόματα των πεδίων που θα προβληθούν
        public override List<string> FieldNamesOf(List<string> fields)
        {
            if (fields == null) return new List<string>();
            if (fields.Any() || fields.Contains("*")) return EntityHelper.GetAllPropertyNames(typeof(Animal)).ToList();

            HashSet<string> projectionFields = new HashSet<string>();
            foreach (string item in fields)
            {
                // Αντιστοιχίζει τα ονόματα πεδίων AnimalDto στα ονόματα πεδίων Animal
                if (item.Equals(nameof(AnimalDto.Id))) projectionFields.Add(nameof(Animal.Id));
                if (item.Equals(nameof(AnimalDto.Name))) projectionFields.Add(nameof(Animal.Name));
                if (item.Equals(nameof(AnimalDto.Description))) projectionFields.Add(nameof(Animal.Description));
                if (item.Equals(nameof(AnimalDto.Gender))) projectionFields.Add(nameof(Animal.Gender));
                if (item.Equals(nameof(AnimalDto.Age))) projectionFields.Add(nameof(Animal.Age));
                if (item.Equals(nameof(AnimalDto.Weight))) projectionFields.Add(nameof(Animal.Weight));
                if (item.Equals(nameof(AnimalDto.AdoptionStatus))) projectionFields.Add(nameof(Animal.AdoptionStatus));
                if (item.Equals(nameof(AnimalDto.HealthStatus))) projectionFields.Add(nameof(Animal.HealthStatus));
                if (item.Equals(nameof(AnimalDto.Photos))) projectionFields.Add(nameof(Animal.Photos));
                if (item.Equals(nameof(AnimalDto.CreatedAt))) projectionFields.Add(nameof(Animal.CreatedAt));
                if (item.Equals(nameof(AnimalDto.UpdatedAt))) projectionFields.Add(nameof(Animal.UpdatedAt));
                if (item.StartsWith(nameof(AnimalDto.Shelter))) projectionFields.Add(nameof(Animal.ShelterId));
                if (item.StartsWith(nameof(AnimalDto.Breed))) projectionFields.Add(nameof(Animal.BreedId));
                if (item.StartsWith(nameof(AnimalDto.Type))) projectionFields.Add(nameof(Animal.TypeId));
            }

            return projectionFields.ToList();
        }
    }
}