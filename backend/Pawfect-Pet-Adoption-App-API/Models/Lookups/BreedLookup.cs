
namespace Pawfect_Pet_Adoption_App_API.Models.Lookups
{
    using Pawfect_Pet_Adoption_App_API.Data.Entities;
    using Pawfect_Pet_Adoption_App_API.Query;
    using Pawfect_Pet_Adoption_App_API.Query.Queries;

    public class BreedLookup : Lookup
    {
        private BreedQuery breedQuery { get; set; }

        // Λίστα με τα αναγνωριστικά των φυλών
        public List<String>? Ids { get; set; }

        public List<String>? ExcludedIds { get; set; }

        // Λίστα με τα αναγνωριστικά των τύπων
        public List<String>? TypeIds { get; set; }

        // Ημερομηνία έναρξης φιλτραρίσματος (δημιουργήθηκε από)
        public DateTime? CreatedFrom { get; set; }

        // Ημερομηνία λήξης φιλτραρίσματος (δημιουργήθηκε μέχρι)
        public DateTime? CreatedTill { get; set; }

        // Εμπλουτίζει το BreedQuery με τα φίλτρα και τις επιλογές του lookup
        // Έξοδος: Το εμπλουτισμένο BreedQuery
        public BreedQuery EnrichLookup(IQueryFactory queryFactory)
        {
            BreedQuery breedQuery = queryFactory.Query<BreedQuery>();

            // Προσθέτει τα φίλτρα στο BreedQuery
            breedQuery.Ids = this.Ids;
            breedQuery.TypeIds = this.TypeIds;
            breedQuery.CreatedFrom = this.CreatedFrom;
            breedQuery.CreatedTill = this.CreatedTill;
            breedQuery.Query = this.Query;

            // Ορίζει επιπλέον επιλογές για το BreedQuery
            breedQuery.PageSize = this.PageSize;
            breedQuery.Offset = this.Offset;
            breedQuery.SortDescending = this.SortDescending;
            breedQuery.Fields = breedQuery.FieldNamesOf(this.Fields.ToList());
            breedQuery.SortBy = this.SortBy;
            breedQuery.ExcludedIds = this.ExcludedIds;

            return breedQuery;
        }

        // Επιστρέφει τον τύπο οντότητας του BreedLookup
        // Έξοδος: Ο τύπος οντότητας του BreedLookup
        public override Type GetEntityType() { return typeof(Breed); }
    }
}
