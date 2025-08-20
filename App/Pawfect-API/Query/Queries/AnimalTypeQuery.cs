
using MongoDB.Bson;
using MongoDB.Driver;

using Pawfect_API.Data.Entities;
using Pawfect_API.Data.Entities.Types.Authorization;
using Pawfect_API.DevTools;
using Pawfect_API.Exceptions;
using Pawfect_API.Models.AnimalType;
using Pawfect_API.Models.Lookups;
using Pawfect_API.Services.AuthenticationServices;
using Pawfect_API.Services.FilterServices;
using Pawfect_API.Services.MongoServices;
using System.Text.RegularExpressions;

namespace Pawfect_API.Query.Queries
{
	public class AnimalTypeQuery : BaseQuery<Data.Entities.AnimalType>
	{
        public AnimalTypeQuery
        (
            MongoDbService mongoDbService,
            IAuthorizationService AuthorizationService,
            ClaimsExtractor claimsExtractor,
            IAuthorizationContentResolver AuthorizationContentResolver

        ) : base(mongoDbService, AuthorizationService, AuthorizationContentResolver, claimsExtractor)
        {
        }

        // Λίστα από IDs τύπων ζώων για φιλτράρισμα
        public List<String>? Ids { get; set; }

        public List<String>? ExcludedIds { get; set; }
        public List<String>? Names { get; set; }

        private AuthorizationFlags _authorise = AuthorizationFlags.None;
        public AnimalTypeQuery Authorise(AuthorizationFlags authorise) { this._authorise = authorise; return this; }

        // Εφαρμόζει τα καθορισμένα φίλτρα στο ερώτημα
        // Έξοδος: FilterDefinition<AnimalType> - ο ορισμός φίλτρου που θα χρησιμοποιηθεί στο ερώτημα
        public override Task<FilterDefinition<Data.Entities.AnimalType>> ApplyFilters()
		{
            FilterDefinitionBuilder<Data.Entities.AnimalType> builder = Builders<Data.Entities.AnimalType>.Filter;
            FilterDefinition<Data.Entities.AnimalType> filter = builder.Empty;

			// Εφαρμόζει φίλτρο για τα IDs των τύπων ζώων
			if (Ids != null && Ids.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = Ids.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In(nameof(Data.Entities.AnimalType.Id), referenceIds.Where(id => id != ObjectId.Empty));
			}

            if (ExcludedIds != null && ExcludedIds.Any())
            {
                // Convert String IDs to ObjectId for comparison
                IEnumerable<ObjectId> referenceIds = ExcludedIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

                // Ensure that only valid ObjectId values are passed in the filter
                filter &= builder.Nin(nameof(Data.Entities.AnimalType.Id), referenceIds.Where(id => id != ObjectId.Empty));
            }

            if (Names != null && Names.Any())
            {
                // Ensure that only valid ObjectId values are passed in the filter
                filter &= builder.In(nameof(Data.Entities.AnimalType.Name), Names);
            }

            // Εφαρμόζει φίλτρο για fuzzy search μέσω indexing στη Mongo σε πεδίο : Name
            if (!String.IsNullOrEmpty(Query))
            {
                List<FilterDefinition<Data.Entities.AnimalType>> searchFilters = new List<FilterDefinition<Data.Entities.AnimalType>>();

                // 1. Standard MongoDB text index search - good for exact and partial matches
                searchFilters.Add(builder.Text(Query));

                String wordBoundaryPattern = $@"\b{Regex.Escape(Query)}";
                BsonRegularExpression wordBoundaryRegex = new BsonRegularExpression(wordBoundaryPattern, "i");
                searchFilters.Add(builder.Regex(nameof(Data.Entities.AnimalType.Name), wordBoundaryRegex));

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
                    searchFilters.Add(builder.Regex(nameof(Data.Entities.AnimalType.Name), fuzzyRegex));
                }

                // Combine all search filters with OR
                filter &= builder.Or(searchFilters);
            }

            return Task.FromResult(filter);
		}

        public override async Task<FilterDefinition<Data.Entities.AnimalType>> ApplyAuthorization(FilterDefinition<Data.Entities.AnimalType> filter)
        {
            if (_authorise.HasFlag(AuthorizationFlags.Permission))
                if (await _authorizationService.AuthorizeAsync(Permission.BrowseAnimalTypes))
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
				// Αντιστοιχίζει τα ονόματα πεδίων AnimalTypeDto στα ονόματα πεδίων AnimalType
				projectionFields.Add(nameof(Data.Entities.AnimalType.Id));
				if (item.Equals(nameof(Models.AnimalType.AnimalType.Name))) projectionFields.Add(nameof(Data.Entities.AnimalType.Name));
				if (item.Equals(nameof(Models.AnimalType.AnimalType.Description))) projectionFields.Add(nameof(Data.Entities.AnimalType.Description));
				if (item.Equals(nameof(Models.AnimalType.AnimalType.CreatedAt))) projectionFields.Add(nameof(Data.Entities.AnimalType.CreatedAt));
				if (item.Equals(nameof(Models.AnimalType.AnimalType.UpdatedAt))) projectionFields.Add(nameof(Data.Entities.AnimalType.UpdatedAt));
			}

			return [.. projectionFields];
		}
	}
}