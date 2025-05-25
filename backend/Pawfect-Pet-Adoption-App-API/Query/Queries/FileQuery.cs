using MongoDB.Bson;
using MongoDB.Driver;
using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Exceptions;
using Pawfect_Pet_Adoption_App_API.Models.AnimalType;
using Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices;
using Pawfect_Pet_Adoption_App_API.Services.FilterServices;
using Pawfect_Pet_Adoption_App_API.Services.MongoServices;
using System.Security.Claims;

namespace Pawfect_Pet_Adoption_App_API.Query.Queries
{
	public class FileQuery : BaseQuery<Data.Entities.File>
	{
        private readonly IFilterBuilder<Data.Entities.File, Models.Lookups.FileLookup> _filterBuilder;

        public FileQuery
        (
            MongoDbService mongoDbService,
            IAuthorisationService authorisationService,
            ClaimsExtractor claimsExtractor,
            IAuthorisationContentResolver authorisationContentResolver,
            IHttpContextAccessor httpContextAccessor,
            IFilterBuilder<Data.Entities.File, Models.Lookups.FileLookup> filterBuilder

        ) : base(mongoDbService, authorisationService, authorisationContentResolver, claimsExtractor, httpContextAccessor)
        {
            _filterBuilder = filterBuilder;
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
				filter &= builder.In("Id", referenceIds.Where(id => id != ObjectId.Empty));
			}

			if (ExcludedIds != null && ExcludedIds.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = ExcludedIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.Nin("Id", referenceIds.Where(id => id != ObjectId.Empty));
			}

			// Owned By Ids
			if (OwnerIds != null && OwnerIds.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = OwnerIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.Nin("OwnerId", referenceIds.Where(id => id != ObjectId.Empty));
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
				filter &= builder.Text(Query);
			}

			return Task.FromResult(filter);
		}

        public override async Task<FilterDefinition<Data.Entities.File>> ApplyAuthorisation(FilterDefinition<Data.Entities.File> filter)
        {
            if (_authorise.HasFlag(AuthorizationFlags.None)) return filter;

            if (_authorise.HasFlag(AuthorizationFlags.Permission))
                if (await _authorisationService.AuthorizeAsync(Permission.BrowseFiles))
                    return filter;

            List<FilterDefinition<Data.Entities.File>> authorizationFilters = new List<FilterDefinition<Data.Entities.File>>();
            if (_authorise.HasFlag(AuthorizationFlags.Owner))
            {
                FilterDefinition<Data.Entities.File> ownedFilter = _authorisationContentResolver.BuildOwnedFilterParams<Data.Entities.File>();
                authorizationFilters.Add(ownedFilter);
            }

            if (authorizationFilters.Count == 0) return filter;

            FilterDefinition<Data.Entities.File> combinedAuthorizationFilter = Builders<Data.Entities.File>.Filter.Or(authorizationFilters);

            filter = Builders<Data.Entities.File>.Filter.And(filter, combinedAuthorizationFilter);

            return await Task.FromResult(filter);
        }

        // Επιστρέφει τα ονόματα πεδίων που θα προβληθούν στο αποτέλεσμα του ερωτήματος
        // Είσοδος: fields - μια λίστα με τα ονόματα των πεδίων που θα προβληθούν
        // Έξοδος: List<String> - τα ονόματα των πεδίων που θα προβληθούν
        public override List<String> FieldNamesOf(List<String> fields)
		{
			if (fields == null || !fields.Any() || fields.Contains("*")) return fields = [.. EntityHelper.GetAllPropertyNames(typeof(Data.Entities.File))];

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
