namespace Pawfect_Pet_Adoption_App_API.Models.Lookups
{
	using Pawfect_Pet_Adoption_App_API.Data.Entities;
	using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
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

            // Προσθέτει τα φίλτρα στο ReportQuery
            reportQuery.Ids = this.Ids;
			reportQuery.ReporteredIds = this.ReporteredIds;
			reportQuery.ReportedIds = this.ReportedIds;
			reportQuery.ReportTypes = this.ReportTypes;
			reportQuery.ReportStatus = this.ReportStatus;
			reportQuery.CreateFrom = this.CreateFrom;
			reportQuery.CreatedTill = this.CreatedTill;
			reportQuery.Query = this.Query;

			// Ορίζει επιπλέον επιλογές για το ReportQuery
			reportQuery.PageSize = this.PageSize;
			reportQuery.Offset = this.Offset;
			reportQuery.SortDescending = this.SortDescending;
			reportQuery.Fields = reportQuery.FieldNamesOf([.. this.Fields]);
			reportQuery.SortBy = this.SortBy;
			reportQuery.ExcludedIds = this.ExcludedIds;

            return reportQuery;
		}

		/// <summary>
		/// Επιστρέφει τον τύπο οντότητας του ReportLookup.
		/// </summary>
		/// <returns>Ο τύπος οντότητας του ReportLookup.</returns>
		public override Type GetEntityType() { return typeof(Report); }
	}
}