
namespace Pawfect_Pet_Adoption_App_API.Models.Lookups
{
    using Pawfect_Pet_Adoption_App_API.Data.Entities;
    using Pawfect_Pet_Adoption_App_API.Query.Queries;

    public class BreedLookup : Lookup
    {
        private BreedQuery _breedQuery { get; set; }

        // Constructor για την κλάση BreedLookup
        // Είσοδος: breedQuery - μια έκδοση της κλάσης BreedQuery
        public BreedLookup(BreedQuery breedQuery)
        {
            _breedQuery = breedQuery;
        }

        public BreedLookup() { }

        // Λίστα με τα αναγνωριστικά των φυλών
        public List<String>? Ids { get; set; }

        // Λίστα με τα αναγνωριστικά των τύπων
        public List<String>? TypeIds { get; set; }

        // Ημερομηνία έναρξης φιλτραρίσματος (δημιουργήθηκε από)
        public DateTime? CreatedFrom { get; set; }

        // Ημερομηνία λήξης φιλτραρίσματος (δημιουργήθηκε μέχρι)
        public DateTime? CreatedTill { get; set; }

        // Εμπλουτίζει το BreedQuery με τα φίλτρα και τις επιλογές του lookup
        // Έξοδος: Το εμπλουτισμένο BreedQuery
        public BreedQuery EnrichLookup(BreedQuery? toEnrichQuery = null)
        {
            if (_breedQuery == null && toEnrichQuery != null)
            {
                _breedQuery = toEnrichQuery;
            }

            // Προσθέτει τα φίλτρα στο BreedQuery
            _breedQuery.Ids = this.Ids;
            _breedQuery.TypeIds = this.TypeIds;
            _breedQuery.CreatedFrom = this.CreatedFrom;
            _breedQuery.CreatedTill = this.CreatedTill;
            _breedQuery.Query = this.Query;

            // Ορίζει επιπλέον επιλογές για το BreedQuery
            _breedQuery.PageSize = this.PageSize;
            _breedQuery.Offset = this.Offset;
            _breedQuery.SortDescending = this.SortDescending;
            _breedQuery.Fields = _breedQuery.FieldNamesOf(this.Fields.ToList());
            _breedQuery.SortBy = this.SortBy;

            return _breedQuery;
        }

        // Επιστρέφει τον τύπο οντότητας του BreedLookup
        // Έξοδος: Ο τύπος οντότητας του BreedLookup
        public override Type GetEntityType() { return typeof(Breed); }
    }
}
