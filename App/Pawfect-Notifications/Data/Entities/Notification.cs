namespace Pawfect_Notifications.Data.Entities
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;
    using Pawfect_Notifications.Data.Entities.EnumTypes;
    using Pawfect_Notifications.Data.Entities.EnumTypes;

    /// <summary>
    /// Το μοντέλο μιας ειδοποίησης στο σύστημα
    /// </summary>
    public class Notification
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public String Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public String UserId { get; set; }
        public NotificationType Type { get; set; }
        public NotificationStatus Status { get; set; }
        public Int32 RetryCount { get; set; } 
        public Int32 MaxRetries { get; set; }
        [BsonIgnoreIfNull]
        public String Title { get; set; }
        [BsonIgnoreIfNull]
        public String Content { get; set; }

        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public Guid TeplateId { get; set; }
        public Dictionary<String, String> TitleMappings { get; set; }
        public Dictionary<String, String> ContentMappings { get; set; }
        public Boolean IsRead { get; set; }

        [BsonIgnoreIfNull]
        public DateTime? ProcessedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
    }
}