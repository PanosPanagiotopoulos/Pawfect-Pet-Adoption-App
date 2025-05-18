using MongoDB.Bson;
using MongoDB.Driver;

using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Models.User;
using Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices;
using Pawfect_Pet_Adoption_App_API.Services.FilterServices;
using Pawfect_Pet_Adoption_App_API.Services.MongoServices;

namespace Pawfect_Pet_Adoption_App_API.Query.Queries
{
	public class UserQuery : BaseQuery<Data.Entities.User>
	{
        private readonly IFilterBuilder<Data.Entities.User, Models.Lookups.UserLookup> _filterBuilder;

        public UserQuery
        (
            MongoDbService mongoDbService,
            IAuthorisationService authorisationService,
            ClaimsExtractor claimsExtractor,
            IAuthorisationContentResolver authorisationContentResolver,
            IFilterBuilder<Data.Entities.User, Models.Lookups.UserLookup> filterBuilder

        ) : base(mongoDbService, authorisationService, authorisationContentResolver, claimsExtractor)
        {
            _filterBuilder = filterBuilder;
        }

        // Λίστα με τα IDs των χρηστών για φιλτράρισμα
        public List<String>? Ids { get; set; }

        public List<String>? ExcludedIds { get; set; }

		public List<String>? ShelterIds { get; set; }

		// Λίστα με τα ονόματα των χρηστών για φιλτράρισμα
		public List<String>? FullNames { get; set; }

		// Λίστα με τους ρόλους των χρηστών για φιλτράρισμα
		public List<UserRole>? Roles { get; set; }

		// Λίστα με τις πόλεις των χρηστών για φιλτράρισμα
		public List<String>? Cities { get; set; }

		// Λίστα με τους ταχυδρομικούς κώδικες των χρηστών για φιλτράρισμα
		public List<String>? Zipcodes { get; set; }

		// Ημερομηνία έναρξης για φιλτράρισμα (δημιουργήθηκε από)
		public DateTime? CreatedFrom { get; set; }

		// Ημερομηνία λήξης για φιλτράρισμα (δημιουργήθηκε μέχρι)
		public DateTime? CreatedTill { get; set; }

        private AuthorizationFlags _authorise = AuthorizationFlags.None;
        public UserQuery Authorise(AuthorizationFlags authorise) { this._authorise = authorise; return this; }

        // Εφαρμόζει τα καθορισμένα φίλτρα στο ερώτημα
        // Έξοδος: FilterDefinition<User> - ο ορισμός φίλτρου που θα χρησιμοποιηθεί στο ερώτημα
        public override Task<FilterDefinition<Data.Entities.User>> ApplyFilters()
		{
            FilterDefinitionBuilder<Data.Entities.User> builder = Builders<Data.Entities.User>.Filter;
            FilterDefinition<Data.Entities.User> filter = builder.Empty;

			// Εφαρμόζει φίλτρο για τα IDs των χρηστών
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

			if (ShelterIds != null && ShelterIds.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = ShelterIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In("ShelterId", referenceIds.Where(id => id != ObjectId.Empty));
			}

			// Εφαρμόζει φίλτρο για τις λεπτομέρειες της αίτησης χρησιμοποιώντας regex
			if (!String.IsNullOrEmpty(Query))
			{
				filter &= builder.Regex(user => user.FullName, new MongoDB.Bson.BsonRegularExpression(Query, "i"));
			}

			// Εφαρμόζει φίλτρο για τα ονόματα των χρηστών
			if (FullNames != null && FullNames.Any())
			{
				filter &= builder.In(user => user.FullName, FullNames);
			}

			// Εφαρμόζει φίλτρο για τους ρόλους των χρηστών
			if (Roles != null && Roles.Any())
			{
				filter &= builder.In(user => user.Role, Roles);
			}

			// Εφαρμόζει φίλτρο για τις πόλεις των χρηστών
			if (Cities != null && Cities.Any())
			{
				filter &= builder.In(user => user.Location.City, Cities);
			}

			// Εφαρμόζει φίλτρο για τους ταχυδρομικούς κώδικες των χρηστών
			if (Zipcodes != null && Zipcodes.Any())
			{
				filter &= builder.In(user => user.Location.ZipCode, Zipcodes);
			}

			// Εφαρμόζει φίλτρο για την ημερομηνία έναρξης
			if (CreatedFrom.HasValue)
			{
				filter &= builder.Gte(user => user.CreatedAt, CreatedFrom.Value);
			}

			// Εφαρμόζει φίλτρο για την ημερομηνία λήξης
			if (CreatedTill.HasValue)
			{
				filter &= builder.Lte(user => user.CreatedAt, CreatedTill.Value);
			}

			// Εφαρμόζει φίλτρο για fuzzy search μέσω indexing στη Mongo σε πεδίο : FullName
			if (!String.IsNullOrEmpty(Query))
			{
				filter &= builder.Text(Query);
			}

			return Task.FromResult(filter);
		}

        public override async Task<FilterDefinition<Data.Entities.User>> ApplyAuthorisation(FilterDefinition<Data.Entities.User> filter)
        {
            return await Task.FromResult(filter);
        }

        // Επιστρέφει τα ονόματα πεδίων που θα προβληθούν στο αποτέλεσμα του ερωτήματος
        // Είσοδος: fields - μια λίστα με τα ονόματα των πεδίων που θα προβληθούν
        // Έξοδος: List<String> - τα ονόματα των πεδίων που θα προβληθούν
        public override List<String> FieldNamesOf(List<String> fields)
		{
			if (fields == null || !fields.Any() || fields.Contains("*")) fields = EntityHelper.GetAllPropertyNames(typeof(Data.Entities.User)).ToList();

			HashSet<String> projectionFields = new HashSet<String>();
			foreach (String item in fields)
			{
				// Αντιστοιχίζει τα ονόματα πεδίων UserDto στα ονόματα πεδίων User
				projectionFields.Add(nameof(Data.Entities.User.Id));
				if (item.Equals(nameof(Models.User.User.Email))) projectionFields.Add(nameof(Data.Entities.User.Email));
				if (item.Equals(nameof(Models.User.User.FullName))) projectionFields.Add(nameof(Data.Entities.User.FullName));
				if (item.Equals(nameof(Models.User.User.Role))) projectionFields.Add(nameof(Data.Entities.User.Role));
				if (item.Equals(nameof(Models.User.User.Phone))) projectionFields.Add(nameof(Data.Entities.User.Phone));
				if (item.Equals(nameof(Models.User.User.Location))) projectionFields.Add(nameof(Data.Entities.User.Location));
				if (item.Equals(nameof(Models.User.User.AuthProvider))) projectionFields.Add(nameof(Data.Entities.User.AuthProvider));
				if (item.Equals(nameof(Models.User.User.AuthProviderId))) projectionFields.Add(nameof(Data.Entities.User.AuthProviderId));
				if (item.StartsWith(nameof(Models.User.User.ProfilePhoto))) projectionFields.Add(nameof(Data.Entities.User.ProfilePhotoId));
				if (item.Equals(nameof(Models.User.User.IsVerified))) projectionFields.Add(nameof(Data.Entities.User.IsVerified));
				if (item.Equals(nameof(Models.User.User.CreatedAt))) projectionFields.Add(nameof(Data.Entities.User.CreatedAt));
				if (item.Equals(nameof(Models.User.User.UpdatedAt))) projectionFields.Add(nameof(Data.Entities.User.UpdatedAt));
				if (item.StartsWith(nameof(Models.User.User.Shelter))) projectionFields.Add(nameof(Data.Entities.User.ShelterId));
			}

			return projectionFields.ToList();
		}
	}
}