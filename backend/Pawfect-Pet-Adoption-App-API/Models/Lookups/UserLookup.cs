namespace Pawfect_Pet_Adoption_App_API.Models.Lookups
{
    using Pawfect_Pet_Adoption_App_API.Data.Entities;
    using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
    using Pawfect_Pet_Adoption_App_API.Query;
    using Pawfect_Pet_Adoption_App_API.Query.Queries;

    public class UserLookup : Lookup
    {
        // Λίστα με τα αναγνωριστικά των χρηστών
        public List<String>? Ids { get; set; }

        public List<String>? ExcludedIds { get; set; }

		public List<String>? ShelterIds { get; set; }

		// Λίστα με τα ονόματα των χρηστών
		public List<String>? FullNames { get; set; }

        // Λίστα με τους ρόλους των χρηστών
        public List<UserRole>? Roles { get; set; }

        // Λίστα με τις πόλεις των χρηστών
        public List<String>? Cities { get; set; }

        // Λίστα με τους ταχυδρομικούς κώδικες των χρηστών
        public List<String>? Zipcodes { get; set; }

        // Ημερομηνία έναρξης για φιλτράρισμα (δημιουργήθηκε από)
        public DateTime? CreatedFrom { get; set; }

        // Ημερομηνία λήξης για φιλτράρισμα (δημιουργήθηκε μέχρι)
        public DateTime? CreatedTill { get; set; }

        /// <summary>
        /// Εμπλουτίζει το UserQuery με τα φίλτρα και τις επιλογές του lookup.
        /// </summary>
        /// <returns>Το εμπλουτισμένο UserQuery.</returns>
        public UserQuery EnrichLookup(IQueryFactory queryFactory)
        {
            UserQuery userQuery = queryFactory.Query<UserQuery>();

            // Προσθέτει τα φίλτρα στο UserQuery
            userQuery.Ids = this.Ids;
            userQuery.ShelterIds = this.ShelterIds;
            userQuery.FullNames = this.FullNames;
            userQuery.Roles = this.Roles;
            userQuery.Cities = this.Cities;
            userQuery.Zipcodes = this.Zipcodes;
            userQuery.CreatedFrom = this.CreatedFrom;
            userQuery.CreatedTill = this.CreatedTill;
            userQuery.Query = this.Query;

            // Ορίζει επιπλέον επιλογές για το UserQuery
            userQuery.PageSize = this.PageSize;
            userQuery.Offset = this.Offset;
            userQuery.SortDescending = this.SortDescending;
            userQuery.Fields = userQuery.FieldNamesOf(this.Fields.ToList());
            userQuery.SortBy = this.SortBy;
            userQuery.ExcludedIds = this.ExcludedIds;

            return userQuery;
        }

        /// <summary>
        /// Επιστρέφει τον τύπο οντότητας του UserLookup.
        /// </summary>
        /// <returns>Ο τύπος οντότητας του UserLookup.</returns>
        public override Type GetEntityType() { return typeof(User); }
    }
}