using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Pawfect_Messenger.Data.Entities.EnumTypes;

namespace Pawfect_Messenger.Data.Entities
{
    /// <summary>
    /// Το μοντέλο ενός μηνύματος στο σύστημα
    /// </summary>
    public class Message
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public String Id { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public String ConversationId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public String SenderId { get; set; }
        public MessageType Type { get; set; }
        public String Content { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonIgnoreIfNull]
        public List<String> ReadBy { get; set; }
        public MessageStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
