
namespace Pawfect_API.Models.Lookups
{
    using MongoDB.Bson;
    using MongoDB.Driver;
    using Pawfect_API.Data.Entities;
    using Pawfect_API.DevTools;
    using Pawfect_API.Query;
    using Pawfect_API.Query.Queries;

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
            if (this.Ids != null && this.Ids.Count != 0) breedQuery.Ids = this.Ids;
            if (this.ExcludedIds != null && this.ExcludedIds.Count != 0) breedQuery.ExcludedIds = this.ExcludedIds;
            if (this.TypeIds != null && this.TypeIds.Count != 0) breedQuery.TypeIds = this.TypeIds;
            if (this.CreatedFrom.HasValue) breedQuery.CreatedFrom = this.CreatedFrom;
            if (this.CreatedTill.HasValue) breedQuery.CreatedTill = this.CreatedTill;
            if (!String.IsNullOrEmpty(this.Query)) breedQuery.Query = this.Query;

            breedQuery.Fields = breedQuery.FieldNamesOf([.. this.Fields]);

            base.EnrichCommon(breedQuery);

            return breedQuery;
        }

        public override async Task<FilterDefinition<BsonDocument>> ToFilters(IQueryFactory queryFactory)
        {
            FilterDefinition<Data.Entities.Breed> filters = await this.EnrichLookup(queryFactory).ApplyFilters();

            return MongoHelper.ToBsonFilter(filters);
        }

        // Επιστρέφει τον τύπο οντότητας του BreedLookup
        // Έξοδος: Ο τύπος οντότητας του BreedLookup
        public override Type GetEntityType() { return typeof(Breed); }
    }
}
