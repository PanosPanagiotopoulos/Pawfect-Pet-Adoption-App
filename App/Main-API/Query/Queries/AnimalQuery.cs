using MongoDB.Bson;
using MongoDB.Driver;

using Main_API.Data.Entities.EnumTypes;
using Main_API.Data.Entities.Types.Authorization;
using Main_API.Exceptions;
using Main_API.Services.AuthenticationServices;
using Main_API.Services.MongoServices;

namespace Main_API.Query.Queries
{
	public class AnimalQuery : BaseQuery<Data.Entities.Animal>
	{

        public AnimalQuery
        (
            MongoDbService mongoDbService,
            IAuthorizationService AuthorizationService,
            ClaimsExtractor claimsExtractor,
            IAuthorizationContentResolver AuthorizationContentResolver

        ) : base(mongoDbService, AuthorizationService, AuthorizationContentResolver, claimsExtractor)
        {
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
        public List<Gender>? Genders { get; set; }
        public double? AgeFrom { get; set; }
        public double? AgeTo { get; set; }

        // Ημερομηνία έναρξης για φιλτράρισμα (δημιουργήθηκε από)
        public DateTime? CreateFrom { get; set; }

		// Ημερομηνία λήξης για φιλτράρισμα (δημιουργήθηκε μέχρι)
		public DateTime? CreatedTill { get; set; }

        private AuthorizationFlags _authorise = AuthorizationFlags.None;

        public AnimalQuery Authorise(AuthorizationFlags authorise) { this._authorise = authorise; return this; }

        // Εφαρμόζει τα καθορισμένα φίλτρα στο ερώτημα
        // Έξοδος: FilterDefinition<Animal> - ο ορισμός φίλτρου που θα χρησιμοποιηθεί στο ερώτημα
        public override async Task<FilterDefinition<Data.Entities.Animal>> ApplyFilters()
		{
            FilterDefinitionBuilder<Data.Entities.Animal> builder = Builders<Data.Entities.Animal>.Filter;
            FilterDefinition<Data.Entities.Animal> filter = builder.Empty;

			// Εφαρμόζει φίλτρο για τα IDs των ζώων
			if (Ids != null && Ids.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = Ids.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In(nameof(Data.Entities.Animal.Id), referenceIds.Where(id => id != ObjectId.Empty));
			}

            if (ExcludedIds != null && ExcludedIds.Any())
            {
                // Convert String IDs to ObjectId for comparison
                IEnumerable<ObjectId> referenceIds = ExcludedIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

                // Ensure that only valid ObjectId values are passed in the filter
                filter &= builder.Nin(nameof(Data.Entities.Animal.Id), referenceIds.Where(id => id != ObjectId.Empty));
            }

            // Εφαρμόζει φίλτρο για τα IDs των καταφυγίων
            if (ShelterIds != null && ShelterIds.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = ShelterIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In(nameof(Data.Entities.Animal.ShelterId), referenceIds.Where(id => id != ObjectId.Empty));
			}

			// Εφαρμόζει φίλτρο για τα IDs των φυλών
			if (BreedIds != null && BreedIds.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = BreedIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In(nameof(Data.Entities.Animal.BreedId), referenceIds.Where(id => id != ObjectId.Empty));
			}

			// Εφαρμόζει φίλτρο για τα IDs των τύπων
			if (TypeIds != null && TypeIds.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = TypeIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In(nameof(Data.Entities.Animal.AnimalTypeId), referenceIds.Where(id => id != ObjectId.Empty));
			}

			// Εφαρμόζει φίλτρο για τις καταστάσεις υιοθεσίας
			if (AdoptionStatuses != null && AdoptionStatuses.Any())
			{
				filter &= builder.In(animal => animal.AdoptionStatus, AdoptionStatuses);
			}

            // Εφαρμόζει φίλτρο για τις καταστάσεις υιοθεσίας
            if (Genders != null && Genders.Any())
            {
                filter &= builder.In(animal => animal.Gender, Genders);
            }

            if (AgeFrom.HasValue)
            {
                filter &= builder.Gte(animal => animal.Age, AgeFrom.Value);
            }

            if (AgeTo.HasValue)
            {
                filter &= builder.Lte(animal => animal.Age, AgeTo.Value);
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
				
			}


			return await Task.FromResult(filter);
		}

        public override async Task<FilterDefinition<Data.Entities.Animal>> ApplyAuthorization(FilterDefinition<Data.Entities.Animal> filter)
        {
            if (_authorise.HasFlag(AuthorizationFlags.Permission))
                if (await _authorizationService.AuthorizeAsync(Permission.BrowseAnimals))
                    return filter;
				else throw new ForbiddenException();

            return await Task.FromResult(filter);
        }

        // Επιστρέφει τα ονόματα πεδίων που θα προβληθούν στο αποτέλεσμα του ερωτήματος
        // Είσοδος: fields - μια λίστα με τα ονόματα των πεδίων που θα προβληθούν
        // Έξοδος: List<String> - τα ονόματα των πεδίων που θα προβληθούν
        public override List<String> FieldNamesOf(List<String> fields)
		{
			if (fields == null || !fields.Any()) return new List<String>();

			HashSet<String> projectionFields = new HashSet<String>();
			foreach (String item in fields)
			{
				// Αντιστοιχίζει τα ονόματα πεδίων AnimalDto στα ονόματα πεδίων Animal
				projectionFields.Add(nameof(Data.Entities.Animal.Id));
				if (item.Equals(nameof(Models.Animal.Animal.Name))) projectionFields.Add(nameof(Data.Entities.Animal.Name));
				if (item.Equals(nameof(Models.Animal.Animal.Description))) projectionFields.Add(nameof(Data.Entities.Animal.Description));
				if (item.Equals(nameof(Models.Animal.Animal.Gender))) projectionFields.Add(nameof(Data.Entities.Animal.Gender));
				if (item.Equals(nameof(Models.Animal.Animal.Age))) projectionFields.Add(nameof(Data.Entities.Animal.Age));
				if (item.Equals(nameof(Models.Animal.Animal.Weight))) projectionFields.Add(nameof(Data.Entities.Animal.Weight));
				if (item.Equals(nameof(Models.Animal.Animal.AdoptionStatus))) projectionFields.Add(nameof(Data.Entities.Animal.AdoptionStatus));
				if (item.Equals(nameof(Models.Animal.Animal.HealthStatus))) projectionFields.Add(nameof(Data.Entities.Animal.HealthStatus));
				if (item.Equals(nameof(Models.Animal.Animal.CreatedAt))) projectionFields.Add(nameof(Data.Entities.Animal.CreatedAt));
				if (item.Equals(nameof(Models.Animal.Animal.UpdatedAt))) projectionFields.Add(nameof(Data.Entities.Animal.UpdatedAt));
                
				if (item.StartsWith(nameof(Models.Animal.Animal.AttachedPhotos))) projectionFields.Add(nameof(Data.Entities.Animal.PhotosIds));
                if (item.StartsWith(nameof(Models.Animal.Animal.Shelter))) projectionFields.Add(nameof(Data.Entities.Animal.ShelterId));
				if (item.StartsWith(nameof(Models.Animal.Animal.Breed))) projectionFields.Add(nameof(Data.Entities.Animal.BreedId));
				if (item.StartsWith(nameof(Models.Animal.Animal.AnimalType))) projectionFields.Add(nameof(Data.Entities.Animal.AnimalTypeId));
			}

			return projectionFields.ToList();
		}
	}
}