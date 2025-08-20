namespace Pawfect_API.Models.Lookups
{
    using MongoDB.Bson;
    using MongoDB.Driver;
    using Pawfect_API.Data.Entities;
    using Pawfect_API.Data.Entities.EnumTypes;
    using Pawfect_API.DevTools;
    using Pawfect_API.Query;
    using Pawfect_API.Query.Queries;

    public class UserLookup : Lookup
    {
        // Λίστα με τα αναγνωριστικά των χρηστών
        public List<String>? Ids { get; set; }

        public List<String>? ExcludedIds { get; set; }

		public List<String>? ShelterIds { get; set; }

		// Λίστα με τα ονόματα των χρηστών
		public List<String>? FullNames { get; set; }

        // Λίστα με τους ρόλους των χρηστών
        public List<UserRole>? Roles { get; set; }

        // Λίστα με τις πόλεις των χρηστών
        public List<String>? Cities { get; set; }

        // Λίστα με τους ταχυδρομικούς κώδικες των χρηστών
        public List<String>? Zipcodes { get; set; }

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
            if (this.Ids != null && this.Ids.Count != 0) userQuery.Ids = this.Ids;
            if (this.ExcludedIds != null && this.ExcludedIds.Count != 0) userQuery.ExcludedIds = this.ExcludedIds;
            if (this.ShelterIds != null && this.ShelterIds.Count != 0) userQuery.ShelterIds = this.ShelterIds;
            if (this.FullNames != null && this.FullNames.Count != 0) userQuery.FullNames = this.FullNames;
            if (this.Roles != null && this.Roles.Count != 0) userQuery.Roles = this.Roles;
            if (this.Cities != null && this.Cities.Count != 0) userQuery.Cities = this.Cities;
            if (this.Zipcodes != null && this.Zipcodes.Count != 0) userQuery.Zipcodes = this.Zipcodes;
            if (this.CreatedFrom.HasValue) userQuery.CreatedFrom = this.CreatedFrom;
            if (this.CreatedTill.HasValue) userQuery.CreatedTill = this.CreatedTill;
            if (!String.IsNullOrEmpty(this.Query)) userQuery.Query = this.Query;

            userQuery.Fields = userQuery.FieldNamesOf([.. this.Fields]);

            base.EnrichCommon(userQuery);

            return userQuery;
        }

        public override async Task<FilterDefinition<BsonDocument>> ToFilters(IQueryFactory queryFactory)
        {
            FilterDefinition<Data.Entities.User> filters = await this.EnrichLookup(queryFactory).ApplyFilters();

            return MongoHelper.ToBsonFilter(filters);
        }

        /// <summary>
        /// Επιστρέφει τον τύπο οντότητας του UserLookup.
        /// </summary>
        /// <returns>Ο τύπος οντότητας του UserLookup.</returns>
        public override Type GetEntityType() { return typeof(User); }
    }
}