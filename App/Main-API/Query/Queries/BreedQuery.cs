using MongoDB.Bson;
using MongoDB.Driver;

using Main_API.Data.Entities;
using Main_API.Data.Entities.Types.Authorization;
using Main_API.DevTools;
using Main_API.Exceptions;
using Main_API.Models.Breed;
using Main_API.Services.AuthenticationServices;
using Main_API.Services.FilterServices;
using Main_API.Services.MongoServices;

namespace Main_API.Query.Queries
{
	public class BreedQuery : BaseQuery<Data.Entities.Breed>
	{
        public BreedQuery
        (
            MongoDbService mongoDbService,
            IAuthorizationService AuthorizationService,
            ClaimsExtractor claimsExtractor,
            IAuthorizationContentResolver AuthorizationContentResolver

        ) : base(mongoDbService, AuthorizationService, AuthorizationContentResolver, claimsExtractor)
        {
        }

        // Λίστα με τα αναγνωριστικά των φυλών για φιλτράρισμα
        public List<String>? Ids { get; set; }

        public List<String>? ExcludedIds { get; set; }


        // Λίστα με τα αναγνωριστικά των τύπων για φιλτράρισμα
        public List<String>? TypeIds { get; set; }

		// Ημερομηνία έναρξης για φιλτράρισμα (δημιουργήθηκε από)
		public DateTime? CreatedFrom { get; set; }

		// Ημερομηνία λήξης για φιλτράρισμα (δημιουργήθηκε μέχρι)
		public DateTime? CreatedTill { get; set; }

        private AuthorizationFlags _authorise = AuthorizationFlags.None;

        public BreedQuery Authorise(AuthorizationFlags authorise) { this._authorise = authorise; return this; }

        // Εφαρμόζει τα καθορισμένα φίλτρα στο ερώτημα
        // Έξοδος: FilterDefinition<Breed> - ο ορισμός φίλτρου που θα χρησιμοποιηθεί στο ερώτημα
        public override Task<FilterDefinition<Data.Entities.Breed>> ApplyFilters()
		{
            FilterDefinitionBuilder<Data.Entities.Breed> builder = Builders<Data.Entities.Breed>.Filter;
            FilterDefinition<Data.Entities.Breed> filter = builder.Empty;

			// Εφαρμόζει φίλτρο για τα αναγνωριστικά των φυλών
			if (Ids != null && Ids.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = Ids.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In(nameof(Data.Entities.Breed.Id), referenceIds.Where(id => id != ObjectId.Empty));
			}

            if (ExcludedIds != null && ExcludedIds.Any())
            {
                // Convert String IDs to ObjectId for comparison
                IEnumerable<ObjectId> referenceIds = ExcludedIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

                // Ensure that only valid ObjectId values are passed in the filter
                filter &= builder.Nin(nameof(Data.Entities.Breed.Id), referenceIds.Where(id => id != ObjectId.Empty));
            }

            // Εφαρμόζει φίλτρο για τα αναγνωριστικά των τύπων
            if (TypeIds != null && TypeIds.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = TypeIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In(nameof(Data.Entities.Breed.AnimalTypeId), referenceIds.Where(id => id != ObjectId.Empty));
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
			if (!String.IsNullOrEmpty(Query))
			{
				filter &= builder.Text(Query);
			}

			return Task.FromResult(filter);
		}

        public override async Task<FilterDefinition<Data.Entities.Breed>> ApplyAuthorization(FilterDefinition<Data.Entities.Breed> filter)
        {
            if (_authorise.HasFlag(AuthorizationFlags.Permission))
                if (await _authorizationService.AuthorizeAsync(Permission.BrowseBreeds))
                    return filter;
                else throw new ForbiddenException();

            return await Task.FromResult(filter);
        }

        // Επιστρέφει τα ονόματα πεδίων που θα προβληθούν στο αποτέλεσμα του ερωτήματος
        // Είσοδος: fields - μια λίστα με τα ονόματα των πεδίων που θα προβληθούν
        // Έξοδος: List<String> - τα ονόματα των πεδίων που θα προβληθούν
        public override List<String> FieldNamesOf(List<String> fields)
		{
			if (fields == null || !fields.Any() || fields.Contains("*")) fields = EntityHelper.GetAllPropertyNames(typeof(Data.Entities.Breed)).ToList();

			HashSet<String> projectionFields = new HashSet<String>();
			foreach (String item in fields)
			{
				projectionFields.Add(nameof(Data.Entities.Breed.Id));
				if (item.Equals(nameof(Models.Breed.Breed.Name))) projectionFields.Add(nameof(Data.Entities.Breed.Name));
				if (item.Equals(nameof(Models.Breed.Breed.Description))) projectionFields.Add(nameof(Data.Entities.Breed.Description));
				if (item.Equals(nameof(Models.Breed.Breed.CreatedAt))) projectionFields.Add(nameof(Data.Entities.Breed.CreatedAt));
				if (item.Equals(nameof(Models.Breed.Breed.UpdatedAt))) projectionFields.Add(nameof(Data.Entities.Breed.UpdatedAt));
				if (item.StartsWith(nameof(Models.Breed.Breed.AnimalType))) projectionFields.Add(nameof(Data.Entities.Breed.AnimalTypeId));
			}
			return projectionFields.ToList();
		}
	}
}