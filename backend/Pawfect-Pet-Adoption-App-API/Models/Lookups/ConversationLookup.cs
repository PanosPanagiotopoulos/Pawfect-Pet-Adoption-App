namespace Pawfect_Pet_Adoption_App_API.Models.Lookups
{
    using Pawfect_Pet_Adoption_App_API.Data.Entities;
    using Pawfect_Pet_Adoption_App_API.Query;
    using Pawfect_Pet_Adoption_App_API.Query.Queries;

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
            conversationQuery.Ids = this.Ids;
            conversationQuery.UserIds = this.UserIds;
            conversationQuery.AnimalIds = this.AnimalIds;
            conversationQuery.CreateFrom = this.CreateFrom;
            conversationQuery.CreatedTill = this.CreatedTill;
            conversationQuery.Query = this.Query;

            // Ορίζει επιπλέον επιλογές για το ConversationQuery
            conversationQuery.PageSize = this.PageSize;
            conversationQuery.Offset = this.Offset;
            conversationQuery.SortDescending = this.SortDescending;
            conversationQuery.Fields = conversationQuery.FieldNamesOf(this.Fields.ToList());
            conversationQuery.SortBy = this.SortBy;
            conversationQuery.ExcludedIds = this.ExcludedIds;

            return conversationQuery;
        }

        // Επιστρέφει τον τύπο οντότητας του ConversationLookup
        // Έξοδος: Ο τύπος οντότητας του ConversationLookup
        public override Type GetEntityType() { return typeof(Conversation); }
    }
}