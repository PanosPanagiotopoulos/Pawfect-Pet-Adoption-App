using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Main_API.Data.Entities.EnumTypes;
using Main_API.Data.Entities.Types.Authorization;
using Main_API.DevTools;
using Main_API.Exceptions;
using Main_API.Models.AnimalType;
using Main_API.Services.AuthenticationServices;
using Main_API.Services.FilterServices;
using Main_API.Services.MongoServices;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Main_API.Query.Queries
{
	public class FileQuery : BaseQuery<Data.Entities.File>
	{

        public FileQuery
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

		public List<String>? OwnerIds { get; set; }

		public List<FileSaveStatus>? FileSaveStatuses { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTill { get; set; }

        public String? Name { get; set; }

        private AuthorizationFlags _authorise = AuthorizationFlags.None;

        public FileQuery Authorise(AuthorizationFlags authorise) { this._authorise = authorise; return this; }

        public override Task<FilterDefinition<Data.Entities.File>> ApplyFilters()
		{
			FilterDefinitionBuilder<Data.Entities.File> builder = Builders<Data.Entities.File>.Filter;
			FilterDefinition<Data.Entities.File> filter = builder.Empty;

			if (Ids != null && Ids.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = Ids.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In(nameof(Data.Entities.File.Id), referenceIds.Where(id => id != ObjectId.Empty));
			}

			if (ExcludedIds != null && ExcludedIds.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = ExcludedIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.Nin(nameof(Data.Entities.File.Id), referenceIds.Where(id => id != ObjectId.Empty));
			}

			// Owned By Ids
			if (OwnerIds != null && OwnerIds.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = OwnerIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.Nin(nameof(Data.Entities.File.OwnerId), referenceIds.Where(id => id != ObjectId.Empty));
			}

			// Owned By Ids
			if (FileSaveStatuses != null && FileSaveStatuses.Any())
			{
				filter &= builder.In(file => file.FileSaveStatus, FileSaveStatuses);
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

            // Εφαρμόζει φίλτρο για fuzzy search μέσω indexing στη Mongo σε πεδίο : Filename
            if (!String.IsNullOrEmpty(Query))
            {
                List<FilterDefinition<Data.Entities.File>> searchFilters = new List<FilterDefinition<Data.Entities.File>>();

                // 1. Standard MongoDB text index search - good for exact and partial matches
                searchFilters.Add(builder.Text(Query));

                String wordBoundaryPattern = $@"\b{Regex.Escape(Query)}";
                BsonRegularExpression wordBoundaryRegex = new BsonRegularExpression(wordBoundaryPattern, "i");
                searchFilters.Add(builder.Regex(nameof(Data.Entities.File.Filename), wordBoundaryRegex));

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
                    searchFilters.Add(builder.Regex(nameof(Data.Entities.File.Filename), fuzzyRegex));
                }

                // Combine all search filters with OR
                filter &= builder.Or(searchFilters);
            }

            return Task.FromResult(filter);
		}

        public override async Task<FilterDefinition<Data.Entities.File>> ApplyAuthorization(FilterDefinition<Data.Entities.File> filter)
        {
            if (_authorise.HasFlag(AuthorizationFlags.None)) return filter;

            if (_authorise.HasFlag(AuthorizationFlags.Permission))
                if (await _authorizationService.AuthorizeAsync(Permission.BrowseFiles))
                    return filter;

            List<FilterDefinition<BsonDocument>> authorizationFilters = new List<FilterDefinition<BsonDocument>>();
            if (_authorise.HasFlag(AuthorizationFlags.Owner))
            {
                FilterDefinition<BsonDocument> ownedFilter = _authorizationContentResolver.BuildOwnedFilterParams(typeof(Data.Entities.File));
                authorizationFilters.Add(ownedFilter);
            }

            if (authorizationFilters.Count == 0) return filter;

            FilterDefinition<BsonDocument> combinedAuthorizationFilter = Builders<BsonDocument>.Filter.Or(authorizationFilters);

            // Combine with the authorization filters (AND logic)
            FilterDefinition<BsonDocument> combinedFinalBsonFilter = Builders<BsonDocument>.Filter.And(MongoHelper.ToBsonFilter<Data.Entities.File>(filter), combinedAuthorizationFilter);

            return await Task.FromResult(MongoHelper.FromBsonFilter<Data.Entities.File>(combinedFinalBsonFilter));
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
				projectionFields.Add(nameof(Data.Entities.File.Id));
                projectionFields.Add(nameof(Data.Entities.File.AwsKey));
                if (item.Equals(nameof(Models.File.File.Filename))) projectionFields.Add(nameof(Data.Entities.File.Filename));
				if (item.Equals(nameof(Models.File.File.FileType))) projectionFields.Add(nameof(Data.Entities.File.FileType));
				if (item.Equals(nameof(Models.File.File.MimeType))) projectionFields.Add(nameof(Data.Entities.File.MimeType));
				if (item.Equals(nameof(Models.File.File.Size))) projectionFields.Add(nameof(Data.Entities.File.Size));
				if (item.Equals(nameof(Models.File.File.FileSaveStatus))) projectionFields.Add(nameof(Data.Entities.File.FileSaveStatus));
				if (item.Equals(nameof(Models.File.File.SourceUrl))) projectionFields.Add(nameof(Data.Entities.File.SourceUrl));
                if (item.Equals(nameof(Models.File.File.CreatedAt))) projectionFields.Add(nameof(Data.Entities.File.CreatedAt));
				if (item.Equals(nameof(Models.File.File.UpdatedAt))) projectionFields.Add(nameof(Data.Entities.File.UpdatedAt));
				if (item.StartsWith(nameof(Models.File.File.Owner))) projectionFields.Add(nameof(Data.Entities.File.OwnerId));

			}

			return projectionFields.ToList();
		}
	}
}
