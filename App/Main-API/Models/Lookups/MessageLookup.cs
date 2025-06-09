namespace Pawfect_Pet_Adoption_App_API.Models.Lookups
{
    using MongoDB.Bson;
    using MongoDB.Driver;
    using Pawfect_Pet_Adoption_App_API.Data.Entities;
    using Pawfect_Pet_Adoption_App_API.DevTools;
    using Pawfect_Pet_Adoption_App_API.Query;
    using Pawfect_Pet_Adoption_App_API.Query.Queries;

    public class MessageLookup : Lookup
    {
        // Λίστα από IDs μηνυμάτων
        public List<String>? Ids { get; set; }

        public List<String>? ExcludedIds { get; set; }


        // Λίστα από IDs συνομιλιών
        public List<String>? ConversationIds { get; set; }

        // Λίστα από IDs αποστολέων
        public List<String>? SenderIds { get; set; }

        // Λίστα από IDs παραληπτών
        public List<String>? RecipientIds { get; set; }

        // Ημερομηνία έναρξης για φιλτράρισμα (δημιουργήθηκε από)
        public DateTime? CreateFrom { get; set; }

        // Ημερομηνία λήξης για φιλτράρισμα (δημιουργήθηκε μέχρι)
        public DateTime? CreatedTill { get; set; }

        // Εμπλουτίζει το MessageQuery με τα φίλτρα και τις επιλογές του lookup
        // Έξοδος: Το εμπλουτισμένο MessageQuery
        public MessageQuery EnrichLookup(IQueryFactory queryFactory)
        {
           MessageQuery messageQuery = queryFactory.Query<MessageQuery>();

            // Προσθέτει φίλτρα στο MessageQuery
            if (this.Ids != null && this.Ids.Count != 0) messageQuery.Ids = this.Ids;
            if (this.ExcludedIds != null && this.ExcludedIds.Count != 0) messageQuery.ExcludedIds = this.ExcludedIds;
            if (this.ConversationIds != null && this.ConversationIds.Count != 0) messageQuery.ConversationIds = this.ConversationIds;
            if (this.SenderIds != null && this.SenderIds.Count != 0) messageQuery.SenderIds = this.SenderIds;
            if (this.RecipientIds != null && this.RecipientIds.Count != 0) messageQuery.RecipientIds = this.RecipientIds;
            if (this.CreateFrom.HasValue) messageQuery.CreateFrom = this.CreateFrom;
            if (this.CreatedTill.HasValue) messageQuery.CreatedTill = this.CreatedTill;
            if (!String.IsNullOrEmpty(this.Query)) messageQuery.Query = this.Query;

            messageQuery.Fields = messageQuery.FieldNamesOf([.. this.Fields]);

            base.EnrichCommon(messageQuery);

            return messageQuery;
        }

        public override async Task<FilterDefinition<BsonDocument>> ToFilters(IQueryFactory queryFactory)
        {
            FilterDefinition<Data.Entities.Message> filters = await this.EnrichLookup(queryFactory).ApplyFilters();

            return MongoHelper.ToBsonFilter(filters);
        }

        // Επιστρέφει τον τύπο οντότητας του MessageLookup
        // Έξοδος: Ο τύπος οντότητας του MessageLookup
        public override Type GetEntityType() { return typeof(Message); }
    }
}