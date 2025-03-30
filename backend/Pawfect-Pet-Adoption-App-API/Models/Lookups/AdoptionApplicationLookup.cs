
using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
using Pawfect_Pet_Adoption_App_API.Query.Queries;

namespace Pawfect_Pet_Adoption_App_API.Models.Lookups
{
    public class AdoptionApplicationLookup : Lookup
    {
        private AdoptionApplicationQuery _adoptionApplicationQuery { get; set; }

        public AdoptionApplicationLookup(AdoptionApplicationQuery adoptionApplicationQuery)
        {
            _adoptionApplicationQuery = adoptionApplicationQuery;
        }

        public AdoptionApplicationLookup() { }

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
        public AdoptionApplicationQuery EnrichLookup(AdoptionApplicationQuery? toEnrichQuery = null)
        {
            if (_adoptionApplicationQuery == null && toEnrichQuery != null)
            {
                _adoptionApplicationQuery = toEnrichQuery;
            }

            // Προσθέτει τα φίλτρα στο AdoptionApplicationQuery
            _adoptionApplicationQuery.Ids = this.Ids;
            _adoptionApplicationQuery.UserIds = this.UserIds;
            _adoptionApplicationQuery.AnimalIds = this.AnimalIds;
            _adoptionApplicationQuery.ShelterIds = this.ShelterIds;
            _adoptionApplicationQuery.Status = this.Status;
            _adoptionApplicationQuery.CreatedFrom = this.CreatedFrom;
            _adoptionApplicationQuery.CreatedTill = this.CreatedTill;
            _adoptionApplicationQuery.Query = this.Query;

            // Ορίζει επιπλέον επιλογές για το AdoptionApplicationQuery
            _adoptionApplicationQuery.PageSize = this.PageSize;
            _adoptionApplicationQuery.Offset = this.Offset;
            _adoptionApplicationQuery.SortDescending = this.SortDescending;
            _adoptionApplicationQuery.Fields = _adoptionApplicationQuery.FieldNamesOf(this.Fields.ToList());
            _adoptionApplicationQuery.SortBy = this.SortBy;
            _adoptionApplicationQuery.ExcludedIds = this.ExcludeIds;

            return _adoptionApplicationQuery;
        }

        /// <summary>
        /// Επιστρέφει τον τύπο οντότητας του AdoptionApplicationLookup.
        /// </summary>
        /// <returns>Ο τύπος οντότητας του AdoptionApplicationLookup.</returns>
        public override Type GetEntityType() { return typeof(AdoptionApplicationLookup); }
    }
}
