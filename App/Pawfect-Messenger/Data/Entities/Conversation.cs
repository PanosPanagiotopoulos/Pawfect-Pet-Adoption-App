namespace Pawfect_Messenger.Data.Entities
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;
    using Pawfect_Messenger.Data.Entities.EnumTypes;

    /// <summary>
    /// Το μοντέλο μιας συνομιλίας στο σύστημα
    /// </summary>
    public class Conversation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public String Id { get; set; }
        public ConversationType Type { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public List<String> Participants { get; set; }

        [BsonIgnoreIfNull]
        public DateTime? LastMessageAt { get; set; }
        public String LastMessageId { get; set; } // Cached for performance

        [BsonRepresentation(BsonType.ObjectId)]
        public String CreatedBy { get; set; } // UserId who created the conversation
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
