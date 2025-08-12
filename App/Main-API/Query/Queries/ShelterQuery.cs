using MongoDB.Bson;
using MongoDB.Driver;

using Main_API.Data.Entities;
using Main_API.Data.Entities.EnumTypes;
using Main_API.Data.Entities.Types.Authorization;
using Main_API.DevTools;
using Main_API.Exceptions;
using Main_API.Models.Shelter;
using Main_API.Services.AuthenticationServices;
using Main_API.Services.FilterServices;
using Main_API.Services.MongoServices;
using System.Text.RegularExpressions;

namespace Main_API.Query.Queries
{
	public class ShelterQuery : BaseQuery<Data.Entities.Shelter>
	{
        public ShelterQuery
        (
            MongoDbService mongoDbService,
            IAuthorizationService AuthorizationService,
            ClaimsExtractor claimsExtractor,
            IAuthorizationContentResolver AuthorizationContentResolver

        ) : base(mongoDbService, AuthorizationService, AuthorizationContentResolver, claimsExtractor)
        {
        }

        // Λίστα με τα IDs των καταφυγίων για φιλτράρισμα
        public List<String>? Ids { get; set; }

        public List<String>? ExcludedIds { get; set; }


        // Λίστα με τα IDs των χρηστών για φιλτράρισμα
        public List<String>? UserIds { get; set; }

		// Λίστα με τις καταστάσεις επιβεβαίωσης για φιλτράρισμα
		public List<VerificationStatus>? VerificationStatuses { get; set; }

		// Λίστα με τα IDs των admin που επιβεβαίωσαν για φιλτράρισμα
		public List<String>? VerifiedBy { get; set; }

        private AuthorizationFlags _authorise = AuthorizationFlags.None;

        public ShelterQuery Authorise(AuthorizationFlags authorise) { this._authorise = authorise; return this; }

        // Εφαρμόζει τα καθορισμένα φίλτρα στο ερώτημα
        // Έξοδος: FilterDefinition<Shelter> - ο ορισμός φίλτρου που θα χρησιμοποιηθεί στο ερώτημα
        public override Task<FilterDefinition<Data.Entities.Shelter>> ApplyFilters()
		{
            FilterDefinitionBuilder<Data.Entities.Shelter> builder = Builders<Data.Entities.Shelter>.Filter;
            FilterDefinition<Data.Entities.Shelter> filter = builder.Empty;

			if (Ids != null && Ids.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = Ids.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In(nameof(Data.Entities.Shelter.Id), referenceIds.Where(id => id != ObjectId.Empty));
			}

            if (ExcludedIds != null && ExcludedIds.Any())
            {
                // Convert String IDs to ObjectId for comparison
                IEnumerable<ObjectId> referenceIds = ExcludedIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

                // Ensure that only valid ObjectId values are passed in the filter
                filter &= builder.Nin(nameof(Data.Entities.Shelter.Id), referenceIds.Where(id => id != ObjectId.Empty));
            }


            // Εφαρμόζει φίλτρο για τα IDs των χρηστών
            if (UserIds != null && UserIds.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = UserIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In(nameof(Data.Entities.Shelter.UserId), referenceIds.Where(id => id != ObjectId.Empty));
			}

			// Εφαρμόζει φίλτρο για τις καταστάσεις επιβεβαίωσης
			if (VerificationStatuses != null && VerificationStatuses.Any())
			{
				filter &= builder.In(shelter => shelter.VerificationStatus, VerificationStatuses);
			}

			// Εφαρμόζει φίλτρο για τα IDs των admin που επιβεβαίωσαν
			if (VerifiedBy != null && VerifiedBy.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = VerifiedBy.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In(nameof(Data.Entities.Shelter.VerifiedById), referenceIds.Where(id => id != ObjectId.Empty));
			}

            // Εφαρμόζει φίλτρο για fuzzy search μέσω indexing στη Mongo σε πεδίο : ShelterName
            if (!String.IsNullOrEmpty(Query))
            {
                List<FilterDefinition<Data.Entities.Shelter>> searchFilters = new List<FilterDefinition<Data.Entities.Shelter>>();

                String normalizedQuery = Query.Trim();
                String escapedQuery = Regex.Escape(normalizedQuery);

                // Exact match (highest priority)
                BsonRegularExpression exactRegex = new BsonRegularExpression($"^{escapedQuery}$", "i");
                searchFilters.Add(builder.Regex(nameof(Data.Entities.Shelter.ShelterName), exactRegex));

                // Starts with query (very high priority)
                BsonRegularExpression startsWithRegex = new BsonRegularExpression($"^{escapedQuery}", "i");
                searchFilters.Add(builder.Regex(nameof(Data.Entities.Shelter.ShelterName), startsWithRegex));

                // Word boundary matching - query matches start of any word
                String wordBoundaryPattern = $@"\b{escapedQuery}";
                BsonRegularExpression wordBoundaryRegex = new BsonRegularExpression(wordBoundaryPattern, "i");
                searchFilters.Add(builder.Regex(nameof(Data.Entities.Shelter.ShelterName), wordBoundaryRegex));

                // Contains query anywhere in name (lower priority)
                BsonRegularExpression containsRegex = new BsonRegularExpression(escapedQuery, "i");
                searchFilters.Add(builder.Regex(nameof(Data.Entities.Shelter.ShelterName), containsRegex));

                // Individual word matching for multi-word queries
                String[] words = normalizedQuery.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (words.Length > 1)
                {
                    // All words must be found (AND logic for individual words)
                    List<FilterDefinition<Data.Entities.Shelter>> wordFilters = new List<FilterDefinition<Data.Entities.Shelter>>();
                    foreach (String word in words)
                    {
                        if (word.Length >= 2) // Ignore very short words
                        {
                            String escapedWord = Regex.Escape(word);
                            String wordPattern = $@"\b{escapedWord}";
                            BsonRegularExpression wordRegex = new BsonRegularExpression(wordPattern, "i");
                            wordFilters.Add(builder.Regex(nameof(Data.Entities.Shelter.ShelterName), wordRegex));
                        }
                    }

                    if (wordFilters.Any())
                    {
                        // All words must be present (stricter matching)
                        searchFilters.Add(builder.And(wordFilters));
                    }
                }

                // Limited fuzzy matching - only for queries 4+ characters and very restrictive
                if (normalizedQuery.Length >= 4)
                {
                    // Only allow one character difference for very close matches
                    String restrictedFuzzyPattern = String.Empty;
                    for (Int32 i = 0; i < escapedQuery.Length; i++)
                    {
                        Char currentChar = escapedQuery[i];
                        if (i == 0)
                        {
                            // First character must match or be close
                            restrictedFuzzyPattern += $"[{currentChar}.]";
                        }
                        else if (i == escapedQuery.Length - 1)
                        {
                            // Last character must match or be close  
                            restrictedFuzzyPattern += $"[{currentChar}.]";
                        }
                        else
                        {
                            // Middle characters must match exactly
                            restrictedFuzzyPattern += currentChar;
                        }
                    }

                    BsonRegularExpression restrictedFuzzyRegex = new BsonRegularExpression(restrictedFuzzyPattern, "i");
                    searchFilters.Add(builder.Regex(nameof(Data.Entities.Shelter.ShelterName), restrictedFuzzyRegex));
                }

                // Combine all search filters with OR
                filter &= builder.Or(searchFilters);
            }

            return Task.FromResult(filter);
		}

        public override async Task<FilterDefinition<Data.Entities.Shelter>> ApplyAuthorization(FilterDefinition<Data.Entities.Shelter> filter)
        {
            if (_authorise.HasFlag(AuthorizationFlags.Permission))
                if (await _authorizationService.AuthorizeAsync(Permission.BrowseShelters))
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
				// Αντιστοιχίζει τα ονόματα πεδίων ShelterDto στα ονόματα πεδίων Shelter
				projectionFields.Add(nameof(Data.Entities.Shelter.Id));
				if (item.StartsWith(nameof(Models.Shelter.Shelter.User))) projectionFields.Add(nameof(Data.Entities.Shelter.UserId));
				if (item.Equals(nameof(Models.Shelter.Shelter.ShelterName))) projectionFields.Add(nameof(Data.Entities.Shelter.ShelterName));
				if (item.Equals(nameof(Models.Shelter.Shelter.Description))) projectionFields.Add(nameof(Data.Entities.Shelter.Description));
				if (item.Equals(nameof(Models.Shelter.Shelter.Website))) projectionFields.Add(nameof(Data.Entities.Shelter.Website));
				if (item.Equals(nameof(Models.Shelter.Shelter.SocialMedia))) projectionFields.Add(nameof(Data.Entities.Shelter.SocialMedia));
				if (item.Equals(nameof(Models.Shelter.Shelter.OperatingHours))) projectionFields.Add(nameof(Data.Entities.Shelter.OperatingHours));
				if (item.Equals(nameof(Models.Shelter.Shelter.VerificationStatus))) projectionFields.Add(nameof(Data.Entities.Shelter.VerificationStatus));
				if (item.Equals(nameof(Models.Shelter.Shelter.VerifiedBy))) projectionFields.Add(nameof(Data.Entities.Shelter.VerifiedById));
			}
			return projectionFields.ToList();
		}
	}
}