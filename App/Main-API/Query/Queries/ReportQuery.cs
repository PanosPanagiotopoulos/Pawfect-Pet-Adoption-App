using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

using Main_API.Data.Entities;
using Main_API.Data.Entities.EnumTypes;
using Main_API.Data.Entities.Types.Authorization;
using Main_API.DevTools;
using Main_API.Exceptions;
using Main_API.Models.Report;
using Main_API.Services.AuthenticationServices;
using Main_API.Services.FilterServices;
using Main_API.Services.MongoServices;
using System.Security.Claims;

namespace Main_API.Query.Queries
{
	public class ReportQuery : BaseQuery<Data.Entities.Report>
	{
        public ReportQuery
        (
            MongoDbService mongoDbService,
            IAuthorizationService AuthorizationService,
            ClaimsExtractor claimsExtractor,
            IAuthorizationContentResolver AuthorizationContentResolver

        ) : base(mongoDbService, AuthorizationService, AuthorizationContentResolver, claimsExtractor)
        {
        }

        // Λίστα με τα IDs των αναφορών για φιλτράρισμα
        public List<String>? Ids { get; set; }

        public List<String>? ExcludedIds { get; set; }


        // Λίστα με τα IDs των χρηστών που αναφέρουν για φιλτράρισμα
        public List<String>? ReporteredIds { get; set; }

		// Λίστα με τα IDs των χρηστών που αναφέρονται για φιλτράρισμα
		public List<String>? ReportedIds { get; set; }

		// Λίστα με τους τύπους αναφορών για φιλτράρισμα
		public List<ReportType>? ReportTypes { get; set; }

		// Λίστα με τις καταστάσεις αναφορών για φιλτράρισμα
		public List<ReportStatus>? ReportStatus { get; set; }

		// Ημερομηνία έναρξης για φιλτράρισμα
		public DateTime? CreateFrom { get; set; }

		// Ημερομηνία λήξης για φιλτράρισμα
		public DateTime? CreatedTill { get; set; }

        private AuthorizationFlags _authorise = AuthorizationFlags.None;

        public ReportQuery Authorise(AuthorizationFlags authorise) { this._authorise = authorise; return this; }

        // Εφαρμόζει τα καθορισμένα φίλτρα στο ερώτημα
        // Έξοδος: FilterDefinition<Report> - ο ορισμός φίλτρου που θα χρησιμοποιηθεί στο ερώτημα
        public override Task<FilterDefinition<Data.Entities.Report>> ApplyFilters()
		{
            FilterDefinitionBuilder<Data.Entities.Report> builder = Builders<Data.Entities.Report>.Filter;
            FilterDefinition<Data.Entities.Report> filter = builder.Empty;

			// Εφαρμόζει φίλτρο για τα IDs των αναφορών
			if (Ids != null && Ids.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = Ids.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In(nameof(Data.Entities.Report.Id), referenceIds.Where(id => id != ObjectId.Empty));
			}

            if (ExcludedIds != null && ExcludedIds.Any())
            {
                // Convert String IDs to ObjectId for comparison
                IEnumerable<ObjectId> referenceIds = ExcludedIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

                // Ensure that only valid ObjectId values are passed in the filter
                filter &= builder.Nin(nameof(Data.Entities.Report.Id), referenceIds.Where(id => id != ObjectId.Empty));
            }

            // Εφαρμόζει φίλτρο για τα IDs των χρηστών που αναφέρουν
            if (ReporteredIds != null && ReporteredIds.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = ReporteredIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In(nameof(Data.Entities.Report.ReporterId), referenceIds.Where(id => id != ObjectId.Empty));
			}

			// Εφαρμόζει φίλτρο για τα IDs των χρηστών που αναφέρονται
			if (ReportedIds != null && ReportedIds.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = ReportedIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);
                
                // Ensure that only valid ObjectId values are passed in the filter
                filter &= builder.In(nameof(Data.Entities.Report.ReportedId), referenceIds.Where(id => id != ObjectId.Empty));
			}

			// Εφαρμόζει φίλτρο για τους τύπους αναφορών
			if (ReportTypes != null && ReportTypes.Any())
			{
				filter &= builder.In(report => report.Type, ReportTypes);
			}

			// Εφαρμόζει φίλτρο για τις καταστάσεις αναφορών
			if (ReportStatus != null && ReportStatus.Any())
			{
				filter &= builder.In(report => report.Status, ReportStatus);
			}

			// Εφαρμόζει φίλτρο για την ημερομηνία έναρξης
			if (CreateFrom.HasValue)
			{
				filter &= builder.Gte(report => report.CreatedAt, CreateFrom.Value);
			}

			// Εφαρμόζει φίλτρο για την ημερομηνία λήξης
			if (CreatedTill.HasValue)
			{
				filter &= builder.Lte(report => report.CreatedAt, CreatedTill.Value);
			}

			return Task.FromResult(filter);
		}

        public override async Task<FilterDefinition<Data.Entities.Report>> ApplyAuthorization(FilterDefinition<Data.Entities.Report> filter)
        {
            if (_authorise.HasFlag(AuthorizationFlags.None)) return filter;

            if (_authorise.HasFlag(AuthorizationFlags.Permission))
                if (await _authorizationService.AuthorizeAsync(Permission.BrowseReports))
                    return filter;

            List<FilterDefinition<BsonDocument>> authorizationFilters = new List<FilterDefinition<BsonDocument>>();
            if (_authorise.HasFlag(AuthorizationFlags.Affiliation))
            {
                FilterDefinition<BsonDocument> affiliatedFilter = await _authorizationContentResolver.BuildAffiliatedFilterParams(typeof(Data.Entities.Report));
                authorizationFilters.Add(affiliatedFilter);
            }

            if (_authorise.HasFlag(AuthorizationFlags.Owner))
            {
                FilterDefinition<BsonDocument> ownedFilter = _authorizationContentResolver.BuildOwnedFilterParams(typeof(Data.Entities.Report));
                authorizationFilters.Add(ownedFilter);
            }

            if (authorizationFilters.Count == 0) return filter;

            FilterDefinition<BsonDocument> combinedAuthorizationFilter = Builders<BsonDocument>.Filter.Or(authorizationFilters);

            // Combine with the authorization filters (AND logic)
            FilterDefinition<BsonDocument> combinedFinalBsonFilter = Builders<BsonDocument>.Filter.And(MongoHelper.ToBsonFilter<Data.Entities.Report>(filter), combinedAuthorizationFilter);

            return await Task.FromResult(MongoHelper.FromBsonFilter<Data.Entities.Report>(combinedFinalBsonFilter));
        }

        // Επιστρέφει τα ονόματα πεδίων που θα προβληθούν στο αποτέλεσμα του ερωτήματος
        // Είσοδος: fields - μια λίστα με τα ονόματα των πεδίων που θα προβληθούν
        // Έξοδος: List<String> - τα ονόματα των πεδίων που θα προβληθούν
        public override List<String> FieldNamesOf(List<String> fields)
		{
			if (fields == null || !fields.Any() || fields.Contains("*")) fields = [.. EntityHelper.GetAllPropertyNames(typeof(Data.Entities.Report))];

			HashSet<String> projectionFields = new HashSet<String>();
			foreach (String item in fields)
			{
				// Αντιστοιχίζει τα ονόματα πεδίων ReportDto στα ονόματα πεδίων Report
				projectionFields.Add(nameof(Data.Entities.Report.Id));
				if (item.Equals(nameof(Models.Report.Report.Reason))) projectionFields.Add(nameof(Data.Entities.Report.Reason));
				if (item.Equals(nameof(Models.Report.Report.Type))) projectionFields.Add(nameof(Data.Entities.Report.Type));
				if (item.Equals(nameof(Models.Report.Report.CreatedAt))) projectionFields.Add(nameof(Data.Entities.Report.CreatedAt));
				if (item.Equals(nameof(Models.Report.Report.UpdatedAt))) projectionFields.Add(nameof(Data.Entities.Report.UpdatedAt));
				if (item.StartsWith(nameof(Models.Report.Report.Reported))) projectionFields.Add(nameof(Data.Entities.Report.ReportedId));
				if (item.StartsWith(nameof(Models.Report.Report.Reporter))) projectionFields.Add(nameof(Data.Entities.Report.ReporterId));
			}
			return projectionFields.ToList();
		}
	}
}