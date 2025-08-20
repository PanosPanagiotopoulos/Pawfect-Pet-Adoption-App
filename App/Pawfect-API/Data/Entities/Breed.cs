using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Pawfect_API.Data.Entities
{
    /// <summary>
    /// Το μοντέλο μιας ράτσας ζώου στο σύστημα
    /// </summary>
    public class Breed
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public String Id { get; set; }

        public String Name { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public String AnimalTypeId { get; set; }
        public String Description { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
