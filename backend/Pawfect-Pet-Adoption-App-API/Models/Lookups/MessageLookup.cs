namespace Pawfect_Pet_Adoption_App_API.Models.Lookups
{
    using Pawfect_Pet_Adoption_App_API.Data.Entities;
    using Pawfect_Pet_Adoption_App_API.Query.Queries;

    public class MessageLookup : Lookup
    {
        private MessageQuery _messageQuery { get; set; }

        // Constructor για την κλάση MessageLookup
        // Είσοδος: messageQuery - μια παρουσία της κλάσης MessageQuery
        public MessageLookup(MessageQuery messageQuery)
        {
            _messageQuery = messageQuery;
        }

        public MessageLookup() { }

        // Λίστα από IDs μηνυμάτων
        public List<String>? Ids { get; set; }

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
        public MessageQuery EnrichLookup(MessageQuery? toEnrichQuery = null)
        {
            if (toEnrichQuery != null && _messageQuery == null)
            {
                _messageQuery = toEnrichQuery;
            }

            // Προσθέτει φίλτρα στο MessageQuery
            _messageQuery.Ids = this.Ids;
            _messageQuery.ConversationIds = this.ConversationIds;
            _messageQuery.SenderIds = this.SenderIds;
            _messageQuery.RecipientIds = this.RecipientIds;
            _messageQuery.CreateFrom = this.CreateFrom;
            _messageQuery.CreatedTill = this.CreatedTill;
            _messageQuery.Query = this.Query;

            // Ορίζει επιπλέον επιλογές για το MessageQuery
            _messageQuery.PageSize = this.PageSize;
            _messageQuery.Offset = this.Offset;
            _messageQuery.SortDescending = this.SortDescending;
            _messageQuery.Fields = _messageQuery.FieldNamesOf(this.Fields.ToList());
            _messageQuery.SortBy = this.SortBy;

            return _messageQuery;
        }

        // Επιστρέφει τον τύπο οντότητας του MessageLookup
        // Έξοδος: Ο τύπος οντότητας του MessageLookup
        public override Type GetEntityType() { return typeof(Message); }
    }
}