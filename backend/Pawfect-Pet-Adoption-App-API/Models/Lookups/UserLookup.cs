namespace Pawfect_Pet_Adoption_App_API.Models.Lookups
{
    using Pawfect_Pet_Adoption_App_API.Data.Entities;
    using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
    using Pawfect_Pet_Adoption_App_API.Query.Queries;

    public class UserLookup : Lookup
    {
        private UserQuery _userQuery { get; set; }

        public UserLookup(UserQuery userQuery)
        {
            _userQuery = userQuery;
        }
        public UserLookup() { }


        // Λίστα με τα αναγνωριστικά των χρηστών
        public List<String>? Ids { get; set; }

        public List<String>? ExcludedIds { get; set; }


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
        public UserQuery EnrichLookup(UserQuery? toEnrichQuery = null)
        {
            if (toEnrichQuery != null && _userQuery == null)
            {
                _userQuery = toEnrichQuery;
            }

            // Προσθέτει τα φίλτρα στο UserQuery
            _userQuery.Ids = this.Ids;
            _userQuery.FullNames = this.FullNames;
            _userQuery.Roles = this.Roles;
            _userQuery.Cities = this.Cities;
            _userQuery.Zipcodes = this.Zipcodes;
            _userQuery.CreatedFrom = this.CreatedFrom;
            _userQuery.CreatedTill = this.CreatedTill;
            _userQuery.Query = this.Query;

            // Ορίζει επιπλέον επιλογές για το UserQuery
            _userQuery.PageSize = this.PageSize;
            _userQuery.Offset = this.Offset;
            _userQuery.SortDescending = this.SortDescending;
            _userQuery.Fields = _userQuery.FieldNamesOf(this.Fields.ToList());
            _userQuery.SortBy = this.SortBy;
            _userQuery.ExcludedIds = this.ExcludedIds;

            return _userQuery;
        }

        /// <summary>
        /// Επιστρέφει τον τύπο οντότητας του UserLookup.
        /// </summary>
        /// <returns>Ο τύπος οντότητας του UserLookup.</returns>
        public override Type GetEntityType() { return typeof(User); }
    }
}