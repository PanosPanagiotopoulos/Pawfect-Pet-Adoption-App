using MongoDB.Driver;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Models.Message;
using Pawfect_Pet_Adoption_App_API.Services;

namespace Pawfect_Pet_Adoption_App_API.Query.Queries
{
    public class MessageQuery : BaseQuery<Message>
    {
        // Constructor για την κλάση MessageQuery
        // Είσοδος: mongoDbService - μια παρουσία της κλάσης MongoDbService
        public MessageQuery(MongoDbService mongoDbService)
        {
            base._collection = mongoDbService.GetCollection<Message>();
        }

        // Λίστα από IDs μηνυμάτων για φιλτράρισμα
        public List<string>? Ids { get; set; }

        // Λίστα από IDs συνομιλιών για φιλτράρισμα
        public List<string>? ConversationIds { get; set; }

        // Λίστα από IDs αποστολέων για φιλτράρισμα
        public List<string>? SenderIds { get; set; }

        // Λίστα από IDs παραληπτών για φιλτράρισμα
        public List<string>? RecipientIds { get; set; }

        // Ημερομηνία έναρξης για φιλτράρισμα (δημιουργήθηκε από)
        public DateTime? CreateFrom { get; set; }

        // Ημερομηνία λήξης για φιλτράρισμα (δημιουργήθηκε μέχρι)
        public DateTime? CreatedTill { get; set; }

        // Εφαρμόζει τα καθορισμένα φίλτρα στο ερώτημα
        // Έξοδος: FilterDefinition<Message> - ο ορισμός του φίλτρου που θα χρησιμοποιηθεί στο ερώτημα
        protected override Task<FilterDefinition<Message>> ApplyFilters()
        {
            FilterDefinitionBuilder<Message> builder = Builders<Message>.Filter;
            FilterDefinition<Message> filter = builder.Empty;

            // Εφαρμόζει φίλτρο για IDs μηνυμάτων
            if (Ids != null && Ids.Any())
            {
                filter &= builder.In(message => message.Id, Ids);
            }

            // Εφαρμόζει φίλτρο για IDs συνομιλιών
            if (ConversationIds != null && ConversationIds.Any())
            {
                filter &= builder.In(message => message.ConversationId, ConversationIds);
            }

            // Εφαρμόζει φίλτρο για IDs αποστολέων
            if (SenderIds != null && SenderIds.Any())
            {
                filter &= builder.In(message => message.SenderId, SenderIds);
            }

            // Εφαρμόζει φίλτρο για IDs παραληπτών
            if (RecipientIds != null && RecipientIds.Any())
            {
                filter &= builder.In(message => message.RecepientId, RecipientIds);
            }

            // Εφαρμόζει φίλτρο για την ημερομηνία έναρξης
            if (CreateFrom.HasValue)
            {
                filter &= builder.Gte(message => message.CreatedAt, CreateFrom.Value);
            }

            // Εφαρμόζει φίλτρο για την ημερομηνία λήξης
            if (CreatedTill.HasValue)
            {
                filter &= builder.Lte(message => message.CreatedAt, CreatedTill.Value);
            }

            return Task.FromResult(filter);
        }

        // Επιστρέφει τα ονόματα των πεδίων που θα προβληθούν στο αποτέλεσμα του ερωτήματος
        // Είσοδος: fields - μια λίστα με τα ονόματα των πεδίων που θα προβληθούν
        // Έξοδος: List<string> - τα ονόματα των πεδίων που θα προβληθούν
        public override List<string> FieldNamesOf(List<string> fields)
        {
            if (fields == null) return new List<string>();
            if (fields.Any() || fields.Contains("*")) return EntityHelper.GetAllPropertyNames(typeof(Message)).ToList();

            HashSet<string> projectionFields = new HashSet<string>();
            foreach (string item in fields)
            {
                // Χαρτογραφεί τα ονόματα των πεδίων του MessageDto στα ονόματα των πεδίων του Message
                if (item.Equals(nameof(MessageDto.Id))) projectionFields.Add(nameof(Message.Id));
                if (item.Equals(nameof(MessageDto.Content))) projectionFields.Add(nameof(Message.Content));
                if (item.Equals(nameof(MessageDto.IsRead))) projectionFields.Add(nameof(Message.IsRead));
                if (item.Equals(nameof(MessageDto.CreatedAt))) projectionFields.Add(nameof(Message.CreatedAt));
                if (item.StartsWith(nameof(MessageDto.Conversation))) projectionFields.Add(nameof(Message.ConversationId));
                if (item.StartsWith(nameof(MessageDto.Sender))) projectionFields.Add(nameof(Message.SenderId));
                if (item.StartsWith(nameof(MessageDto.Recipient))) projectionFields.Add(nameof(Message.RecepientId));
            }
            return projectionFields.ToList();
        }
    }
}