using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Pawfect_Pet_Adoption_App_API.Data.Entities
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

        [BsonRepresentation(BsonType.ObjectId)]
        public String RecipientId { get; set; }

        public String Content { get; set; }

        public Boolean IsRead { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
