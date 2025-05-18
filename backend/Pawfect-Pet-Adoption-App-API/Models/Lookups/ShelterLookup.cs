namespace Pawfect_Pet_Adoption_App_API.Models.Lookups
{
    using Pawfect_Pet_Adoption_App_API.Data.Entities;
    using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
    using Pawfect_Pet_Adoption_App_API.Query;
    using Pawfect_Pet_Adoption_App_API.Query.Queries;

    public class ShelterLookup : Lookup
    {
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
        public ShelterQuery EnrichLookup(IQueryFactory queryFactory)
        {
            ShelterQuery shelterQuery = queryFactory.Query<ShelterQuery>();

            // Προσθέτει τα φίλτρα στο ShelterQuery
            shelterQuery.Ids = this.Ids;
            shelterQuery.UserIds = this.UserIds;
            shelterQuery.VerificationStatuses = this.VerificationStatuses;
            shelterQuery.VerifiedBy = this.VerifiedBy;
            shelterQuery.Query = this.Query;

            // Ορίζει επιπλέον επιλογές για το ShelterQuery
            shelterQuery.PageSize = this.PageSize;
            shelterQuery.Offset = this.Offset;
            shelterQuery.SortDescending = this.SortDescending;
            shelterQuery.Fields = shelterQuery.FieldNamesOf([.. this.Fields]);
            shelterQuery.SortBy = this.SortBy;
            shelterQuery.ExcludedIds = this.ExcludedIds;

            return shelterQuery;
        }
        public override Type GetEntityType() { return typeof(Shelter); }
    }
}
