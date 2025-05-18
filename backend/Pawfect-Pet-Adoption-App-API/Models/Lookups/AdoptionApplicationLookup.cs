
using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
using Pawfect_Pet_Adoption_App_API.Query;
using Pawfect_Pet_Adoption_App_API.Query.Queries;

namespace Pawfect_Pet_Adoption_App_API.Models.Lookups
{
    public class AdoptionApplicationLookup : Lookup
    {
        // Λίστα με τα αναγνωριστικά των αιτήσεων υιοθεσίας
        public List<String>? Ids { get; set; }

        public List<String>? ExcludedIds { get; set; }

        // Λίστα με τα αναγνωριστικά των χρηστών
        public List<String>? UserIds { get; set; }
        // Λίστα με τα αναγνωριστικά των ζώων
        public List<String>? AnimalIds { get; set; }
        // Λίστα με τα αναγνωριστικά των καταφυγίων

        public List<String>? ShelterIds { get; set; }
        // Λίστα με τις καταστάσεις υιοθεσίας
        public List<AdoptionStatus>? Status { get; set; }
        // Ημερομηνία έναρξης για φιλτράρισμα (δημιουργήθηκε από)
        public DateTime? CreatedFrom { get; set; }

        // Ημερομηνία λήξης για φιλτράρισμα (δημιουργήθηκε μέχρι)
        public DateTime? CreatedTill { get; set; }

        /// <summary>
        /// Εμπλουτίζει το AdoptionApplicationQuery με τα φίλτρα και τις επιλογές του lookup.
        /// </summary>
        /// <returns>Το εμπλουτισμένο AdoptionApplicationQuery.</returns>
        public AdoptionApplicationQuery EnrichLookup(IQueryFactory queryFactory)
        {
            AdoptionApplicationQuery adoptionApplicationQuery = queryFactory.Query<AdoptionApplicationQuery>();

            // Προσθέτει τα φίλτρα στο AdoptionApplicationQuery
            adoptionApplicationQuery.Ids = this.Ids;
            adoptionApplicationQuery.UserIds = this.UserIds;
            adoptionApplicationQuery.AnimalIds = this.AnimalIds;
            adoptionApplicationQuery.ShelterIds = this.ShelterIds;
            adoptionApplicationQuery.Status = this.Status;
            adoptionApplicationQuery.CreatedFrom = this.CreatedFrom;
            adoptionApplicationQuery.CreatedTill = this.CreatedTill;
            adoptionApplicationQuery.Query = this.Query;

            // Ορίζει επιπλέον επιλογές για το AdoptionApplicationQuery
            adoptionApplicationQuery.PageSize = this.PageSize;
            adoptionApplicationQuery.Offset = this.Offset;
            adoptionApplicationQuery.SortDescending = this.SortDescending;
            adoptionApplicationQuery.Fields = adoptionApplicationQuery.FieldNamesOf([.. this.Fields]);
            adoptionApplicationQuery.SortBy = this.SortBy;
            adoptionApplicationQuery.ExcludedIds = this.ExcludeIds;

            return adoptionApplicationQuery;
        }

        /// <summary>
        /// Επιστρέφει τον τύπο οντότητας του AdoptionApplicationLookup.
        /// </summary>
        /// <returns>Ο τύπος οντότητας του AdoptionApplicationLookup.</returns>
        public override Type GetEntityType() { return typeof(AdoptionApplicationLookup); }
    }
}
