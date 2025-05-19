using MongoDB.Bson;
using MongoDB.Driver;

using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Exceptions;
using Pawfect_Pet_Adoption_App_API.Models.Report;
using Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices;
using Pawfect_Pet_Adoption_App_API.Services.FilterServices;
using Pawfect_Pet_Adoption_App_API.Services.MongoServices;
using System.Security.Claims;

namespace Pawfect_Pet_Adoption_App_API.Query.Queries
{
	public class ReportQuery : BaseQuery<Data.Entities.Report>
	{
        private readonly IFilterBuilder<Data.Entities.Report, Models.Lookups.ReportLookup> _filterBuilder;

        public ReportQuery
        (
            MongoDbService mongoDbService,
            IAuthorisationService authorisationService,
            ClaimsExtractor claimsExtractor,
            IAuthorisationContentResolver authorisationContentResolver,
            IFilterBuilder<Data.Entities.Report, Models.Lookups.ReportLookup> filterBuilder

        ) : base(mongoDbService, authorisationService, authorisationContentResolver, claimsExtractor)
        {
            _filterBuilder = filterBuilder;
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
				filter &= builder.In("Id", referenceIds.Where(id => id != ObjectId.Empty));
			}

            if (ExcludedIds != null && ExcludedIds.Any())
            {
                // Convert String IDs to ObjectId for comparison
                IEnumerable<ObjectId> referenceIds = ExcludedIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

                // Ensure that only valid ObjectId values are passed in the filter
                filter &= builder.Nin("Id", referenceIds.Where(id => id != ObjectId.Empty));
            }

            // Εφαρμόζει φίλτρο για τα IDs των χρηστών που αναφέρουν
            if (ReporteredIds != null && ReporteredIds.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = ReporteredIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In("ReporterId", referenceIds.Where(id => id != ObjectId.Empty));
			}

			// Εφαρμόζει φίλτρο για τα IDs των χρηστών που αναφέρονται
			if (ReportedIds != null && ReportedIds.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = ReportedIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In("ReportedId", referenceIds.Where(id => id != ObjectId.Empty));
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

        public override async Task<FilterDefinition<Data.Entities.Report>> ApplyAuthorisation(FilterDefinition<Data.Entities.Report> filter)
        {
            if (_authorise.HasFlag(AuthorizationFlags.None)) return filter;

            if (_authorise.HasFlag(AuthorizationFlags.Permission))
                if (await _authorisationService.AuthorizeAsync(Permission.BrowseReports))
                    return filter;

            List<FilterDefinition<Data.Entities.Report>> authorizationFilters = new List<FilterDefinition<Data.Entities.Report>>();
            if (_authorise.HasFlag(AuthorizationFlags.Affiliation))
            {
                FilterDefinition<Data.Entities.Report> affiliatedFilter = _authorisationContentResolver.BuildAffiliatedFilterParams<Data.Entities.Report>();
                authorizationFilters.Add(affiliatedFilter);
            }

            if (_authorise.HasFlag(AuthorizationFlags.Owner))
            {
                FilterDefinition<Data.Entities.Report> ownedFilter = _authorisationContentResolver.BuildOwnedFilterParams<Data.Entities.Report>();
                authorizationFilters.Add(ownedFilter);
            }

            if (authorizationFilters.Count == 0) return filter;

            FilterDefinition<Data.Entities.Report> combinedAuthorizationFilter = Builders<Data.Entities.Report>.Filter.Or(authorizationFilters);

            filter = Builders<Data.Entities.Report>.Filter.And(filter, combinedAuthorizationFilter);

            return await Task.FromResult(filter);
        }

        // Επιστρέφει τα ονόματα πεδίων που θα προβληθούν στο αποτέλεσμα του ερωτήματος
        // Είσοδος: fields - μια λίστα με τα ονόματα των πεδίων που θα προβληθούν
        // Έξοδος: List<String> - τα ονόματα των πεδίων που θα προβληθούν
        public override List<String> FieldNamesOf(List<String> fields)
		{
			if (fields == null || !fields.Any() || fields.Contains("*")) fields = EntityHelper.GetAllPropertyNames(typeof(Data.Entities.Report)).ToList();

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