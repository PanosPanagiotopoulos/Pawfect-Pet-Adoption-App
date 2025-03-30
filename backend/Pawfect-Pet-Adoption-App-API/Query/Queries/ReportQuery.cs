using MongoDB.Bson;
using MongoDB.Driver;

using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Models.Report;
using Pawfect_Pet_Adoption_App_API.Services.MongoServices;

namespace Pawfect_Pet_Adoption_App_API.Query.Queries
{
	public class ReportQuery : BaseQuery<Report>
	{
		// Κατασκευαστής για την κλάση ReportQuery
		// Είσοδος: mongoDbService - μια έκδοση της κλάσης MongoDbService
		public ReportQuery(MongoDbService mongoDbService)
		{
			base._collection = mongoDbService.GetCollection<Report>();
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

		// Εφαρμόζει τα καθορισμένα φίλτρα στο ερώτημα
		// Έξοδος: FilterDefinition<Report> - ο ορισμός φίλτρου που θα χρησιμοποιηθεί στο ερώτημα
		protected override Task<FilterDefinition<Report>> ApplyFilters()
		{
			FilterDefinitionBuilder<Report> builder = Builders<Report>.Filter;
			FilterDefinition<Report> filter = builder.Empty;

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

		// Επιστρέφει τα ονόματα πεδίων που θα προβληθούν στο αποτέλεσμα του ερωτήματος
		// Είσοδος: fields - μια λίστα με τα ονόματα των πεδίων που θα προβληθούν
		// Έξοδος: List<String> - τα ονόματα των πεδίων που θα προβληθούν
		public override List<String> FieldNamesOf(List<String> fields)
		{
			if (fields == null || !fields.Any() || fields.Contains("*")) fields = EntityHelper.GetAllPropertyNames(typeof(ReportDto)).ToList();

			HashSet<String> projectionFields = new HashSet<String>();
			foreach (String item in fields)
			{
				// Αντιστοιχίζει τα ονόματα πεδίων ReportDto στα ονόματα πεδίων Report
				projectionFields.Add(nameof(Report.Id));
				if (item.Equals(nameof(ReportDto.Reason))) projectionFields.Add(nameof(Report.Reason));
				if (item.Equals(nameof(ReportDto.Type))) projectionFields.Add(nameof(Report.Type));
				if (item.Equals(nameof(ReportDto.CreatedAt))) projectionFields.Add(nameof(Report.CreatedAt));
				if (item.Equals(nameof(ReportDto.UpdatedAt))) projectionFields.Add(nameof(Report.UpdatedAt));
				if (item.StartsWith(nameof(ReportDto.Reported))) projectionFields.Add(nameof(Report.ReportedId));
				if (item.StartsWith(nameof(ReportDto.Reporter))) projectionFields.Add(nameof(Report.ReporterId));
			}
			return projectionFields.ToList();
		}
	}
}