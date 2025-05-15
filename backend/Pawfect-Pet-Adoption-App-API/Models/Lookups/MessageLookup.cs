namespace Pawfect_Pet_Adoption_App_API.Models.Lookups
{
    using Pawfect_Pet_Adoption_App_API.Data.Entities;
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
            messageQuery.Ids = this.Ids;
            messageQuery.ConversationIds = this.ConversationIds;
            messageQuery.SenderIds = this.SenderIds;
            messageQuery.RecipientIds = this.RecipientIds;
            messageQuery.CreateFrom = this.CreateFrom;
            messageQuery.CreatedTill = this.CreatedTill;
            messageQuery.Query = this.Query;

            // Ορίζει επιπλέον επιλογές για το MessageQuery
            messageQuery.PageSize = this.PageSize;
            messageQuery.Offset = this.Offset;
            messageQuery.SortDescending = this.SortDescending;
            messageQuery.Fields = messageQuery.FieldNamesOf(this.Fields.ToList());
            messageQuery.SortBy = this.SortBy;
            messageQuery.ExcludedIds = this.ExcludedIds;

            return messageQuery;
        }

        // Επιστρέφει τον τύπο οντότητας του MessageLookup
        // Έξοδος: Ο τύπος οντότητας του MessageLookup
        public override Type GetEntityType() { return typeof(Message); }
    }
}