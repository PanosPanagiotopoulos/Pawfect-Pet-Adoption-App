
using MongoDB.Bson;
using MongoDB.Driver;
using Main_API.Data.Entities.EnumTypes;
using Main_API.DevTools;
using Main_API.Query;
using Main_API.Query.Queries;

namespace Main_API.Models.Lookups
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
            if (this.Ids != null && this.Ids.Count != 0) adoptionApplicationQuery.Ids = this.Ids;
            if (this.ExcludedIds != null && this.ExcludedIds.Count != 0) adoptionApplicationQuery.ExcludedIds = this.ExcludedIds;
            if (this.UserIds != null && this.UserIds.Count != 0) adoptionApplicationQuery.UserIds = this.UserIds;
            if (this.AnimalIds != null && this.AnimalIds.Count != 0) adoptionApplicationQuery.AnimalIds = this.AnimalIds;
            if (this.ShelterIds != null && this.ShelterIds.Count != 0) adoptionApplicationQuery.ShelterIds = this.ShelterIds;
            if (this.Status != null && this.Status.Count != 0) adoptionApplicationQuery.Status = this.Status;
            if (this.CreatedFrom.HasValue) adoptionApplicationQuery.CreatedFrom = this.CreatedFrom;
            if (this.CreatedTill.HasValue) adoptionApplicationQuery.CreatedTill = this.CreatedTill;
            if (!String.IsNullOrEmpty(this.Query)) adoptionApplicationQuery.Query = this.Query;

            adoptionApplicationQuery.Fields = adoptionApplicationQuery.FieldNamesOf([.. this.Fields]);

            base.EnrichCommon(adoptionApplicationQuery);

            return adoptionApplicationQuery;
        }

        public override async Task<FilterDefinition<BsonDocument>> ToFilters(IQueryFactory queryFactory)
        {
            FilterDefinition<Data.Entities.AdoptionApplication> filters = await this.EnrichLookup(queryFactory).ApplyFilters();

            return MongoHelper.ToBsonFilter(filters);
        }

        /// <summary>
        /// Επιστρέφει τον τύπο οντότητας του AdoptionApplicationLookup.
        /// </summary>
        /// <returns>Ο τύπος οντότητας του AdoptionApplicationLookup.</returns>
        public override Type GetEntityType() { return typeof(Data.Entities.AdoptionApplication); }
    }
}
