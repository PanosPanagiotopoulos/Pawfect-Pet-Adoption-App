namespace Main_API.Models.Lookups
{
    using MongoDB.Bson;
    using MongoDB.Driver;
    using Main_API.Data.Entities;
    using Main_API.DevTools;
    using Main_API.Query;
    using Main_API.Query.Queries;
    using ZstdSharp.Unsafe;

    public class ConversationLookup : Lookup
    {
        // Λίστα με τα IDs των συνομιλιών
        public List<String>? Ids { get; set; }

        public List<String>? ExcludedIds { get; set; }


        // Λίστα με τα IDs των χρηστών
        public List<String>? UserIds { get; set; }

        // Λίστα με τα IDs των ζώων
        public List<String>? AnimalIds { get; set; }

        // Ημερομηνία έναρξης για φιλτράρισμα (δημιουργήθηκε από)
        public DateTime? CreateFrom { get; set; }

        // Ημερομηνία λήξης για φιλτράρισμα (δημιουργήθηκε μέχρι)
        public DateTime? CreatedTill { get; set; }

        // Εμπλουτίζει το ConversationQuery με τα φίλτρα και τις επιλογές του lookup
        // Έξοδος: Το εμπλουτισμένο ConversationQuery
        public ConversationQuery EnrichLookup(IQueryFactory queryFactory)
        {
            ConversationQuery conversationQuery = queryFactory.Query<ConversationQuery>();

            // Προσθέτει φίλτρα στο ConversationQuery
            if (this.Ids != null && this.Ids.Count != 0) conversationQuery.Ids = this.Ids;
            if (this.ExcludedIds != null && this.ExcludedIds.Count != 0) conversationQuery.ExcludedIds = this.ExcludedIds;
            if (this.UserIds != null && this.UserIds.Count != 0) conversationQuery.UserIds = this.UserIds;
            if (this.AnimalIds != null && this.AnimalIds.Count != 0) conversationQuery.AnimalIds = this.AnimalIds;
            if (this.CreateFrom.HasValue) conversationQuery.CreateFrom = this.CreateFrom;
            if (this.CreatedTill.HasValue) conversationQuery.CreatedTill = this.CreatedTill;
            if (!String.IsNullOrEmpty(this.Query)) conversationQuery.Query = this.Query;

            // Ορίζει επιπλέον επιλογές για το ConversationQuery
            conversationQuery.Fields = conversationQuery.FieldNamesOf([.. this.Fields]);

            base.EnrichCommon(conversationQuery);

            return conversationQuery;
        }

        public override async Task<FilterDefinition<BsonDocument>> ToFilters(IQueryFactory queryFactory)
        {
            FilterDefinition<Data.Entities.Conversation> filters = await this.EnrichLookup(queryFactory).ApplyFilters();

            return MongoHelper.ToBsonFilter(filters);
        }

        // Επιστρέφει τον τύπο οντότητας του ConversationLookup
        // Έξοδος: Ο τύπος οντότητας του ConversationLookup
        public override Type GetEntityType() { return typeof(Conversation); }
    }
}