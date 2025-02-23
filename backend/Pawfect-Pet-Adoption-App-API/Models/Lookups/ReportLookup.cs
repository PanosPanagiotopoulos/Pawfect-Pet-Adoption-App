namespace Pawfect_Pet_Adoption_App_API.Models.Lookups
{
	using Pawfect_Pet_Adoption_App_API.Data.Entities;
	using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
	using Pawfect_Pet_Adoption_App_API.Query.Queries;

	public class ReportLookup : Lookup
	{
		private ReportQuery _reportQuery { get; set; }

		public ReportLookup(ReportQuery reportQuery)
		{
			_reportQuery = reportQuery;
		}

		public ReportLookup() { }

		// Λίστα με τα αναγνωριστικά των αναφορών
		public List<String>? Ids { get; set; }

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
		public ReportQuery EnrichLookup(ReportQuery? toEnrichQuery = null)
		{
			if (toEnrichQuery != null && _reportQuery == null)
			{
				_reportQuery = toEnrichQuery;
			}
			// Προσθέτει τα φίλτρα στο ReportQuery
			_reportQuery.Ids = this.Ids;
			_reportQuery.ReporteredIds = this.ReporteredIds;
			_reportQuery.ReportedIds = this.ReportedIds;
			_reportQuery.ReportTypes = this.ReportTypes;
			_reportQuery.ReportStatus = this.ReportStatus;
			_reportQuery.CreateFrom = this.CreateFrom;
			_reportQuery.CreatedTill = this.CreatedTill;
			_reportQuery.Query = this.Query;

			// Ορίζει επιπλέον επιλογές για το ReportQuery
			_reportQuery.PageSize = this.PageSize;
			_reportQuery.Offset = this.Offset;
			_reportQuery.SortDescending = this.SortDescending;
			_reportQuery.Fields = _reportQuery.FieldNamesOf(this.Fields.ToList());
			_reportQuery.SortBy = this.SortBy;

			return _reportQuery;
		}

		/// <summary>
		/// Επιστρέφει τον τύπο οντότητας του ReportLookup.
		/// </summary>
		/// <returns>Ο τύπος οντότητας του ReportLookup.</returns>
		public override Type GetEntityType() { return typeof(Report); }
	}
}