namespace Pawfect_Messenger.Models.Lookups
{
    using MongoDB.Bson;
    using MongoDB.Driver;
    using Pawfect_Messenger.Data.Entities.EnumTypes;
    using Pawfect_Messenger.DevTools;
    using Pawfect_Messenger.Query;
    using Pawfect_Messenger.Query.Queries;

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

            // Προσθέτει τα φίλτρα στο ShelterQuery με if statements
            if (this.Ids != null && this.Ids.Count != 0) shelterQuery.Ids = this.Ids;
            if (this.ExcludedIds != null && this.ExcludedIds.Count != 0) shelterQuery.ExcludedIds = this.ExcludedIds;
            if (this.UserIds != null && this.UserIds.Count != 0) shelterQuery.UserIds = this.UserIds;
            if (this.VerificationStatuses != null && this.VerificationStatuses.Count != 0) shelterQuery.VerificationStatuses = this.VerificationStatuses;
            if (this.VerifiedBy != null && this.VerifiedBy.Count != 0) shelterQuery.VerifiedBy = this.VerifiedBy;
            if (!String.IsNullOrEmpty(this.Query)) shelterQuery.Query = this.Query;

            shelterQuery.Fields = shelterQuery.FieldNamesOf([.. this.Fields]);

            base.EnrichCommon(shelterQuery);

            return shelterQuery;
        }

        public override async Task<FilterDefinition<BsonDocument>> ToFilters(IQueryFactory queryFactory)
        {
            FilterDefinition<Data.Entities.Shelter> filters = await this.EnrichLookup(queryFactory).ApplyFilters();

            return MongoHelper.ToBsonFilter(filters);
        }
        public override Type GetEntityType() { return typeof(Data.Entities.Shelter); }
    }
}
