namespace Pawfect_Messenger.Models.Lookups
{
    using MongoDB.Bson;
    using MongoDB.Driver;

    using Pawfect_Messenger.Data.Entities;
    using Pawfect_Messenger.Data.Entities.EnumTypes;
    using Pawfect_Messenger.DevTools;
    using Pawfect_Messenger.Query;
    using Pawfect_Messenger.Query.Queries;

    public class ConversationLookup : Lookup
    {
        // Λίστα με τα IDs των συνομιλιών
        public List<String> Ids { get; set; }

        public List<String> ExcludedIds { get; set; }


        // Λίστα με τα IDs των χρηστών
        public List<String> Participants { get; set; }
        public List<ConversationType> Types { get; set; }

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
            if (Ids != null && Ids.Count != 0) conversationQuery.Ids = Ids;
            if (ExcludedIds != null && ExcludedIds.Count != 0) conversationQuery.ExcludedIds = ExcludedIds;
            if (Participants != null && Participants.Count != 0) conversationQuery.Participants = Participants;
            if (Types != null && Types.Count != 0) conversationQuery.Types = Types;
            if (CreateFrom.HasValue) conversationQuery.CreateFrom = CreateFrom;
            if (CreatedTill.HasValue) conversationQuery.CreatedTill = CreatedTill;
            if (!String.IsNullOrEmpty(Query)) conversationQuery.Query = Query;

            // Ορίζει επιπλέον επιλογές για το ConversationQuery
            conversationQuery.Fields = conversationQuery.FieldNamesOf([.. Fields]);

            base.EnrichCommon(conversationQuery);

            return conversationQuery;
        }

        public override async Task<FilterDefinition<BsonDocument>> ToFilters(IQueryFactory queryFactory)
        {
            FilterDefinition<Conversation> filters = await EnrichLookup(queryFactory).ApplyFilters();

            return MongoHelper.ToBsonFilter(filters);
        }

        // Επιστρέφει τον τύπο οντότητας του ConversationLookup
        // Έξοδος: Ο τύπος οντότητας του ConversationLookup
        public override Type GetEntityType() { return typeof(Conversation); }
    }
}