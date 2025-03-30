namespace Pawfect_Pet_Adoption_App_API.Models.Lookups
{
    using Pawfect_Pet_Adoption_App_API.Data.Entities;
    using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
    using Pawfect_Pet_Adoption_App_API.Query.Queries;

    public class ShelterLookup : Lookup
    {
        private ShelterQuery _shelterQuery { get; set; }

        public ShelterLookup(ShelterQuery shelterQuery)
        {
            _shelterQuery = shelterQuery;
        }

        public ShelterLookup() { }

        // Λίστα με τα αναγνωριστικά των καταφυγίων
        public List<String>? Ids { get; set; }

        public List<String>? ExcludedIds { get; set; }


        // Λίστα με τα αναγνωριστικά των χρηστών
        public List<String>? UserIds { get; set; }

        // Λίστα με τις καταστάσεις επιβεβαίωσης
        public List<VerificationStatus>? VerificationStatuses { get; set; }

        // Λίστα με τα αναγνωριστικά των admin που επιβεβαίωσαν
        public List<String>? VerifiedBy { get; set; }

        /// <summary>
        /// Εμπλουτίζει το ShelterQuery με τα φίλτρα και τις επιλογές του lookup.
        /// </summary>
        /// <returns>Το εμπλουτισμένο ShelterQuery.</returns>
        public ShelterQuery EnrichLookup(ShelterQuery? toEnrichQuery = null)
        {
            if (toEnrichQuery != null && _shelterQuery == null)
            {
                _shelterQuery = toEnrichQuery;
            }
            // Προσθέτει τα φίλτρα στο ShelterQuery
            _shelterQuery.Ids = this.Ids;
            _shelterQuery.UserIds = this.UserIds;
            _shelterQuery.VerificationStatuses = this.VerificationStatuses;
            _shelterQuery.VerifiedBy = this.VerifiedBy;
            _shelterQuery.Query = this.Query;

            // Ορίζει επιπλέον επιλογές για το ShelterQuery
            _shelterQuery.PageSize = this.PageSize;
            _shelterQuery.Offset = this.Offset;
            _shelterQuery.SortDescending = this.SortDescending;
            _shelterQuery.Fields = _shelterQuery.FieldNamesOf(this.Fields.ToList());
            _shelterQuery.SortBy = this.SortBy;
            _shelterQuery.ExcludedIds = this.ExcludedIds;

            return _shelterQuery;
        }
        public override Type GetEntityType() { return typeof(Shelter); }
    }
}
