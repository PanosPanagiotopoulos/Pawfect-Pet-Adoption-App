namespace Pawfect_Messenger.Models.Lookups
{
    using MongoDB.Bson;
    using MongoDB.Driver;
    using Pawfect_Messenger.Data.Entities;
    using Pawfect_Messenger.Data.Entities.EnumTypes;
    using Pawfect_Messenger.DevTools;
    using Pawfect_Messenger.Query;
    using Pawfect_Messenger.Query.Queries;

    public class MessageLookup : Lookup
    {
        public List<String> Ids { get; set; }

        public List<String> ExcludedIds { get; set; }

        // Λίστα από IDs συνομιλιών για φιλτράρισμα
        public List<String> ConversationIds { get; set; }

        // Λίστα από IDs αποστολέων για φιλτράρισμα
        public List<String> SenderIds { get; set; }

        // Λίστα από IDs παραληπτών για φιλτράρισμα
        public List<MessageType> Types { get; set; }
        public List<String> ReadyBy { get; set; }
        public List<MessageStatus> Statuses { get; set; }

        // Ημερομηνία έναρξης για φιλτράρισμα (δημιουργήθηκε από)
        public DateTime? CreateFrom { get; set; }

        // Ημερομηνία λήξης για φιλτράρισμα (δημιουργήθηκε μέχρι)
        public DateTime? CreatedTill { get; set; }

        public MessageQuery EnrichLookup(IQueryFactory queryFactory)
        {
           MessageQuery messageQuery = queryFactory.Query<MessageQuery>();

            // Προσθέτει φίλτρα στο MessageQuery
            if (Ids != null && Ids.Count != 0) messageQuery.Ids = Ids;
            if (ExcludedIds != null && ExcludedIds.Count != 0) messageQuery.ExcludedIds = ExcludedIds;
            if (ConversationIds != null && ConversationIds.Count != 0) messageQuery.ConversationIds = ConversationIds;
            if (SenderIds != null && SenderIds.Count != 0) messageQuery.SenderIds = SenderIds;
            if (ReadyBy != null && ReadyBy.Count != 0) messageQuery.ReadyBy = ReadyBy;
            if (Types != null && Types.Count != 0) messageQuery.Types = Types;
            if (Statuses != null && Statuses.Count != 0) messageQuery.Statuses = Statuses;
            if (CreateFrom.HasValue) messageQuery.CreateFrom = CreateFrom;
            if (CreatedTill.HasValue) messageQuery.CreatedTill = CreatedTill;
            if (!String.IsNullOrEmpty(Query)) messageQuery.Query = Query;

            messageQuery.Fields = messageQuery.FieldNamesOf([.. Fields]);

            base.EnrichCommon(messageQuery);

            return messageQuery;
        }

        public override async Task<FilterDefinition<BsonDocument>> ToFilters(IQueryFactory queryFactory)
        {
            FilterDefinition<Message> filters = await EnrichLookup(queryFactory).ApplyFilters();

            return MongoHelper.ToBsonFilter(filters);
        }

        // Επιστρέφει τον τύπο οντότητας του MessageLookup
        // Έξοδος: Ο τύπος οντότητας του MessageLookup
        public override Type GetEntityType() { return typeof(Message); }
    }
}