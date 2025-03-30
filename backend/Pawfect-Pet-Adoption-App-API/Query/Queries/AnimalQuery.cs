// FILE: Query/Queries/AnimalQuery.cs
using MongoDB.Bson;
using MongoDB.Driver;

using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Models.Animal;
using Pawfect_Pet_Adoption_App_API.Services.MongoServices;
using Pawfect_Pet_Adoption_App_API.Services.SearchServices;
using Serilog;

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
		public List<String>? Ids { get; set; }

        public List<String>? ExcludedIds { get; set; }


        // Λίστα από IDs καταφυγίων για φιλτράρισμα
        public List<String>? ShelterIds { get; set; }

		// Λίστα από IDs φυλών για φιλτράρισμα
		public List<String>? BreedIds { get; set; }

		// Λίστα από IDs τύπων για φιλτράρισμα
		public List<String>? TypeIds { get; set; }

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
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = Ids.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In("Id", referenceIds.Where(id => id != ObjectId.Empty));
			}

            if (ExcludedIds != null && ExcludedIds.Any())
            {
                // Convert String IDs to ObjectId for comparison
                IEnumerable<ObjectId> referenceIds = ExcludedIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

                // Ensure that only valid ObjectId values are passed in the filter
                filter &= builder.Nin("Id", referenceIds.Where(id => id != ObjectId.Empty));
            }

            // Εφαρμόζει φίλτρο για τα IDs των καταφυγίων
            if (ShelterIds != null && ShelterIds.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = ShelterIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In("ShelterId", referenceIds.Where(id => id != ObjectId.Empty));
			}

			// Εφαρμόζει φίλτρο για τα IDs των φυλών
			if (BreedIds != null && BreedIds.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = BreedIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In("BreedId", referenceIds.Where(id => id != ObjectId.Empty));
			}

			// Εφαρμόζει φίλτρο για τα IDs των τύπων
			if (TypeIds != null && TypeIds.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = TypeIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In("AnimalTypeId", referenceIds.Where(id => id != ObjectId.Empty));
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
			if (!String.IsNullOrEmpty(Query))
			{
				//// Χρήση υπηρεσίας search για να βρούμε τα ids των ζώων που ταιριάζουν στο query
				//SearchRequest searchRequest = new SearchRequest { Query = Query, TopK = base.PageSize, Lang = "el" };
				//List<String>? ids = ((await _searchService.SearchAnimalsAsync(searchRequest)) ?? new List<AnimalIndexModel>())
				//					.Select(searchRes => searchRes.Id).ToList();
				//// Φιλτράρουμε με αυτά τα ids
				//if (ids != null && ids.Any())
				//{
				//	filter &= builder.In(animal => animal.Id, ids);
				//}
			}


			return await Task.FromResult(filter);
		}

		// Επιστρέφει τα ονόματα πεδίων που θα προβληθούν στο αποτέλεσμα του ερωτήματος
		// Είσοδος: fields - μια λίστα με τα ονόματα των πεδίων που θα προβληθούν
		// Έξοδος: List<String> - τα ονόματα των πεδίων που θα προβληθούν
		public override List<String> FieldNamesOf(List<String> fields)
		{
			if (fields == null || !fields.Any() || fields.Contains("*")) fields = EntityHelper.GetAllPropertyNames(typeof(AnimalDto)).ToList();

			HashSet<String> projectionFields = new HashSet<String>();
			foreach (String item in fields)
			{
				// Αντιστοιχίζει τα ονόματα πεδίων AnimalDto στα ονόματα πεδίων Animal
				projectionFields.Add(nameof(Animal.Id));
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
				if (item.StartsWith(nameof(AnimalDto.AnimalType))) projectionFields.Add(nameof(Animal.AnimalTypeId));
			}

			return projectionFields.ToList();
		}
	}
}