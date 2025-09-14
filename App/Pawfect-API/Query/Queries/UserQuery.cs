using MongoDB.Bson;
using MongoDB.Driver;
using Pawfect_API.Data.Entities.EnumTypes;
using Pawfect_API.Data.Entities.Types.Authorization;
using Pawfect_API.Exceptions;
using Pawfect_API.Services.AuthenticationServices;
using Pawfect_API.Services.MongoServices;
using System.Text.RegularExpressions;

namespace Pawfect_API.Query.Queries
{
	public class UserQuery : BaseQuery<Data.Entities.User>
	{
        public UserQuery
        (
            MongoDbService mongoDbService,
            IAuthorizationService AuthorizationService,
            ClaimsExtractor claimsExtractor,
            IAuthorizationContentResolver AuthorizationContentResolver,
            IHttpContextAccessor httpContextAccessor

        ) : base(mongoDbService, AuthorizationService, AuthorizationContentResolver, claimsExtractor)
        {
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

        public Boolean? IsVerified { get; set; }

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
				filter &= builder.In(nameof(Data.Entities.User.Id), referenceIds.Where(id => id != ObjectId.Empty));
			}

            if (ExcludedIds != null && ExcludedIds.Any())
            {
                // Convert String IDs to ObjectId for comparison
                IEnumerable<ObjectId> referenceIds = ExcludedIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

                // Ensure that only valid ObjectId values are passed in the filter
                filter &= builder.Nin(nameof(Data.Entities.User.Id), referenceIds.Where(id => id != ObjectId.Empty));
            }

			if (ShelterIds != null && ShelterIds.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = ShelterIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In(nameof(Data.Entities.User.ShelterId), referenceIds.Where(id => id != ObjectId.Empty));
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
                filter &= builder.AnyIn(user => user.Roles, this.Roles);
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

			if (IsVerified.HasValue)
            {
                filter &= builder.Eq(user => user.IsVerified, IsVerified.Value);
            }

            // Εφαρμόζει φίλτρο για fuzzy search μέσω indexing στη Mongo σε πεδίο : FullName
            if (!String.IsNullOrEmpty(Query))
            {
                List<FilterDefinition<Data.Entities.User>> searchFilters = new List<FilterDefinition<Data.Entities.User>>();

                // 1. Standard MongoDB text index search - good for exact and partial matches
                searchFilters.Add(builder.Text(Query));

                String wordBoundaryPattern = $@"\b{Regex.Escape(Query)}";
                BsonRegularExpression wordBoundaryRegex = new BsonRegularExpression(wordBoundaryPattern, "i");
                searchFilters.Add(builder.Regex(nameof(Data.Entities.User.FullName), wordBoundaryRegex));

                // 3. Character-level fuzzy matching (handles minor typos) - only for longer queries
                if (Query.Length >= 3)
                {
                    String fuzzyPattern = String.Empty;
                    String escapedFuzzyQuery = Regex.Escape(Query);

                    for (Int32 i = 0; i < escapedFuzzyQuery.Length; i++)
                    {
                        Char currentChar = escapedFuzzyQuery[i];

                        // Add the current character with optional preceding character (handles insertions)
                        fuzzyPattern += $".?{currentChar}";

                        // Allow for character substitution (replace with any character)
                        if (i < escapedFuzzyQuery.Length - 1)
                        {
                            fuzzyPattern += "?";
                        }
                    }

                    BsonRegularExpression fuzzyRegex = new BsonRegularExpression(fuzzyPattern, "i");
                    searchFilters.Add(builder.Regex(nameof(Data.Entities.User.FullName), fuzzyRegex));
                }

                // Combine all search filters with OR
                filter &= builder.Or(searchFilters);
            }

            return Task.FromResult(filter);
		}

        public override async Task<FilterDefinition<Data.Entities.User>> ApplyAuthorization(FilterDefinition<Data.Entities.User> filter)
        {
            if (_authorise.HasFlag(AuthorizationFlags.Permission))
                if (await _authorizationService.AuthorizeAsync(Permission.BrowseUsers))
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
				// Αντιστοιχίζει τα ονόματα πεδίων UserDto στα ονόματα πεδίων User
				projectionFields.Add(nameof(Data.Entities.User.Id));
                projectionFields.Add(nameof(Data.Entities.User.Roles));

                if (item.Equals(nameof(Models.User.User.Email))) projectionFields.Add(nameof(Data.Entities.User.Email));
				if (item.Equals(nameof(Models.User.User.FullName))) projectionFields.Add(nameof(Data.Entities.User.FullName));
				if (item.Equals(nameof(Models.User.User.Roles))) projectionFields.Add(nameof(Data.Entities.User.Roles));
				if (item.Equals(nameof(Models.User.User.IsVerified))) projectionFields.Add(nameof(Data.Entities.User.IsVerified));
				if (item.Equals(nameof(Models.User.User.CreatedAt))) projectionFields.Add(nameof(Data.Entities.User.CreatedAt));
				if (item.Equals(nameof(Models.User.User.UpdatedAt))) projectionFields.Add(nameof(Data.Entities.User.UpdatedAt));

				// Sensitive info. Include user role to build correctly
				if (item.Equals(nameof(Models.User.User.Phone))) projectionFields.Add(nameof(Data.Entities.User.Phone));
                if (item.Equals(nameof(Models.User.User.Location))) projectionFields.Add(nameof(Data.Entities.User.Location));

                // Foreign info
                if (item.StartsWith(nameof(Models.User.User.ProfilePhoto))) projectionFields.Add(nameof(Data.Entities.User.ProfilePhotoId));
                if (item.StartsWith(nameof(Models.User.User.Shelter))) projectionFields.Add(nameof(Data.Entities.User.ShelterId));
			}

			return [.. projectionFields];
		}
	}
}