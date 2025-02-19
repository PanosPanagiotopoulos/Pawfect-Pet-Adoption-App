namespace Pawfect_Pet_Adoption_App_API.Data.Entities
{

    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;
    using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;

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

        public String Content { get; set; }

        public Boolean IsRead { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}