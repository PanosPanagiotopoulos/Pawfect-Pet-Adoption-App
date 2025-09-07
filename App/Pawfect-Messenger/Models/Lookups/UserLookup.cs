namespace Pawfect_Messenger.Models.Lookups
{
    using MongoDB.Bson;
    using MongoDB.Driver;
    using Pawfect_Messenger.Data.Entities;
    using Pawfect_Messenger.Data.Entities.EnumTypes;
    using Pawfect_Messenger.DevTools;
    using Pawfect_Messenger.Query;
    using Pawfect_Messenger.Query.Queries;

    public class UserLookup : Lookup
    {
        // Λίστα με τα αναγνωριστικά των χρηστών
        public List<String> Ids { get; set; }

        public List<String> ExcludedIds { get; set; }

		public List<String> ShelterIds { get; set; }

		// Λίστα με τα ονόματα των χρηστών
		public List<String> FullNames { get; set; }

        // Λίστα με τους ρόλους των χρηστών
        public List<UserRole> Roles { get; set; }

        // Λίστα με τις πόλεις των χρηστών
        public List<String> Cities { get; set; }

        // Λίστα με τους ταχυδρομικούς κώδικες των χρηστών
        public List<String> Zipcodes { get; set; }

        // Ημερομηνία έναρξης για φιλτράρισμα (δημιουργήθηκε από)
        public DateTime? CreatedFrom { get; set; }

        // Ημερομηνία λήξης για φιλτράρισμα (δημιουργήθηκε μέχρι)
        public DateTime? CreatedTill { get; set; }

        /// <summary>
        /// Εμπλουτίζει το UserQuery με τα φίλτρα και τις επιλογές του lookup.
        /// </summary>
        /// <returns>Το εμπλουτισμένο UserQuery.</returns>
        public UserQuery EnrichLookup(IQueryFactory queryFactory)
        {
            UserQuery userQuery = queryFactory.Query<UserQuery>();

            // Προσθέτει τα φίλτρα στο UserQuery με if statements
            if (Ids != null && Ids.Count != 0) userQuery.Ids = Ids;
            if (ExcludedIds != null && ExcludedIds.Count != 0) userQuery.ExcludedIds = ExcludedIds;
            if (ShelterIds != null && ShelterIds.Count != 0) userQuery.ShelterIds = ShelterIds;
            if (FullNames != null && FullNames.Count != 0) userQuery.FullNames = FullNames;
            if (Roles != null && Roles.Count != 0) userQuery.Roles = Roles;
            if (Cities != null && Cities.Count != 0) userQuery.Cities = Cities;
            if (Zipcodes != null && Zipcodes.Count != 0) userQuery.Zipcodes = Zipcodes;
            if (CreatedFrom.HasValue) userQuery.CreatedFrom = CreatedFrom;
            if (CreatedTill.HasValue) userQuery.CreatedTill = CreatedTill;
            if (!String.IsNullOrEmpty(Query)) userQuery.Query = Query;

            userQuery.Fields = userQuery.FieldNamesOf([.. Fields]);

            base.EnrichCommon(userQuery);

            return userQuery;
        }

        public override async Task<FilterDefinition<BsonDocument>> ToFilters(IQueryFactory queryFactory)
        {
            FilterDefinition<User> filters = await EnrichLookup(queryFactory).ApplyFilters();

            return MongoHelper.ToBsonFilter(filters);
        }

        /// <summary>
        /// Επιστρέφει τον τύπο οντότητας του UserLookup.
        /// </summary>
        /// <returns>Ο τύπος οντότητας του UserLookup.</returns>
        public override Type GetEntityType() { return typeof(User); }
    }
}