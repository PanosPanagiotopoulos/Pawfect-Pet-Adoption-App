namespace Pawfect_Pet_Adoption_App_API.Models.Lookups
{
    using MongoDB.Bson;
    using MongoDB.Driver;
    using Pawfect_Pet_Adoption_App_API.Data.Entities;
	using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
    using Pawfect_Pet_Adoption_App_API.DevTools;
    using Pawfect_Pet_Adoption_App_API.Query;
    using Pawfect_Pet_Adoption_App_API.Query.Queries;

	public class ReportLookup : Lookup
	{
		public ReportLookup() { }

		// Λίστα με τα αναγνωριστικά των αναφορών
		public List<String>? Ids { get; set; }

        public List<String>? ExcludedIds { get; set; }


        // Λίστα με τα αναγνωριστικά των αναφέροντων χρηστών
        public List<String>? ReporteredIds { get; set; }

		// Λίστα με τα αναγνωριστικά των αναφερόμενων χρηστών
		public List<String>? ReportedIds { get; set; }

		// Λίστα με τους τύπους των αναφορών
		public List<ReportType>? ReportTypes { get; set; }

		// Λίστα με τα καταστήματα κατάστασης των αναφορών
		public List<ReportStatus>? ReportStatus { get; set; }

		// Ημερομηνία έναρξης για φιλτράρισμα (δημιουργήθηκε από)
		public DateTime? CreateFrom { get; set; }

		// Ημερομηνία λήξης για φιλτράρισμα (δημιουργήθηκε μέχρι)
		public DateTime? CreatedTill { get; set; }

        /// <summary>
        /// Εμπλουτίζει το ReportQuery με τα φίλτρα και τις επιλογές του lookup.
        /// </summary>
        /// <returns>Το εμπλουτισμένο ReportQuery.</returns>
        public ReportQuery EnrichLookup(IQueryFactory queryFactory)
        {
            ReportQuery reportQuery = queryFactory.Query<ReportQuery>();

            // Προσθέτει τα φίλτρα στο ReportQuery με if statements
            if (this.Ids != null && this.Ids.Count != 0) reportQuery.Ids = this.Ids;
            if (this.ExcludedIds != null && this.ExcludedIds.Count != 0) reportQuery.ExcludedIds = this.ExcludedIds;
            if (this.ReporteredIds != null && this.ReporteredIds.Count != 0) reportQuery.ReporteredIds = this.ReporteredIds;
            if (this.ReportedIds != null && this.ReportedIds.Count != 0) reportQuery.ReportedIds = this.ReportedIds;
            if (this.ReportTypes != null && this.ReportTypes.Count != 0) reportQuery.ReportTypes = this.ReportTypes;
            if (this.ReportStatus != null && this.ReportStatus.Count != 0) reportQuery.ReportStatus = this.ReportStatus;
            if (this.CreateFrom.HasValue) reportQuery.CreateFrom = this.CreateFrom;
            if (this.CreatedTill.HasValue) reportQuery.CreatedTill = this.CreatedTill;
            if (!String.IsNullOrEmpty(this.Query)) reportQuery.Query = this.Query;

            reportQuery.Fields = reportQuery.FieldNamesOf([.. this.Fields]);

            base.EnrichCommon(reportQuery);

            return reportQuery;
        }

        public override async Task<FilterDefinition<BsonDocument>> ToFilters(IQueryFactory queryFactory)
        {
            FilterDefinition<Data.Entities.Report> filters = await this.EnrichLookup(queryFactory).ApplyFilters();

            return MongoHelper.ToBsonFilter(filters);
        }

        /// <summary>
        /// Επιστρέφει τον τύπο οντότητας του ReportLookup.
        /// </summary>
        /// <returns>Ο τύπος οντότητας του ReportLookup.</returns>
        public override Type GetEntityType() { return typeof(Report); }
	}
}