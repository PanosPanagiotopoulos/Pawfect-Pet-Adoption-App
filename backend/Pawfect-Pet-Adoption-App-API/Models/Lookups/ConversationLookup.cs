namespace Pawfect_Pet_Adoption_App_API.Models.Lookups
{
    using Pawfect_Pet_Adoption_App_API.Data.Entities;
    using Pawfect_Pet_Adoption_App_API.Query.Queries;

    public class ConversationLookup : Lookup
    {
        private ConversationQuery _conversationQuery { get; set; }

        // Constructor για την κλάση ConversationLookup
        // Είσοδος: conversationQuery - μια έκδοση της κλάσης ConversationQuery
        public ConversationLookup(ConversationQuery conversationQuery)
        {
            _conversationQuery = conversationQuery;
        }

        public ConversationLookup() { }

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
        public ConversationQuery EnrichLookup(ConversationQuery? toEnrichQuery = null)
        {
            if (_conversationQuery == null && toEnrichQuery != null)
            {
                _conversationQuery = toEnrichQuery;
            }

            // Προσθέτει φίλτρα στο ConversationQuery
            _conversationQuery.Ids = this.Ids;
            _conversationQuery.UserIds = this.UserIds;
            _conversationQuery.AnimalIds = this.AnimalIds;
            _conversationQuery.CreateFrom = this.CreateFrom;
            _conversationQuery.CreatedTill = this.CreatedTill;
            _conversationQuery.Query = this.Query;

            // Ορίζει επιπλέον επιλογές για το ConversationQuery
            _conversationQuery.PageSize = this.PageSize;
            _conversationQuery.Offset = this.Offset;
            _conversationQuery.SortDescending = this.SortDescending;
            _conversationQuery.Fields = _conversationQuery.FieldNamesOf(this.Fields.ToList());
            _conversationQuery.SortBy = this.SortBy;
            _conversationQuery.ExcludedIds = this.ExcludedIds;

            return _conversationQuery;
        }

        // Επιστρέφει τον τύπο οντότητας του ConversationLookup
        // Έξοδος: Ο τύπος οντότητας του ConversationLookup
        public override Type GetEntityType() { return typeof(Conversation); }
    }
}