using MongoDB.Bson;
using MongoDB.Driver;

using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Models.Breed;
using Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices;
using Pawfect_Pet_Adoption_App_API.Services.FilterServices;
using Pawfect_Pet_Adoption_App_API.Services.MongoServices;

namespace Pawfect_Pet_Adoption_App_API.Query.Queries
{
	public class BreedQuery : BaseQuery<Data.Entities.Breed>
	{
        private readonly IFilterBuilder<Data.Entities.Breed, Models.Lookups.BreedLookup> _filterBuilder;

        public BreedQuery
        (
            MongoDbService mongoDbService,
            IAuthorisationService authorisationService,
            ClaimsExtractor claimsExtractor,
            IAuthorisationContentResolver authorisationContentResolver,
            IHttpContextAccessor httpContextAccessor,
            IFilterBuilder<Data.Entities.Breed, Models.Lookups.BreedLookup> filterBuilder

        ) : base(mongoDbService, authorisationService, authorisationContentResolver, claimsExtractor, httpContextAccessor)
        {
            _filterBuilder = filterBuilder;
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
				filter &= builder.In("Id", referenceIds.Where(id => id != ObjectId.Empty));
			}

            if (ExcludedIds != null && ExcludedIds.Any())
            {
                // Convert String IDs to ObjectId for comparison
                IEnumerable<ObjectId> referenceIds = ExcludedIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

                // Ensure that only valid ObjectId values are passed in the filter
                filter &= builder.Nin("Id", referenceIds.Where(id => id != ObjectId.Empty));
            }

            // Εφαρμόζει φίλτρο για τα αναγνωριστικά των τύπων
            if (TypeIds != null && TypeIds.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = TypeIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In("AnimalTypeId", referenceIds.Where(id => id != ObjectId.Empty));
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

        public override async Task<FilterDefinition<Data.Entities.Breed>> ApplyAuthorisation(FilterDefinition<Data.Entities.Breed> filter)
        {
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