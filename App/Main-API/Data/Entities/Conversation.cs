namespace Main_API.Data.Entities
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    /// <summary>
    /// Το μοντέλο μιας συνομιλίας στο σύστημα
    /// </summary>
    public class Conversation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public String Id { get; set; }

        /// <value>
        /// Τα id των χρηστών της συζήτησης
        /// </value>
        [BsonRepresentation(BsonType.ObjectId)]
        public List<String> UserIds { get; set; }

        /// <value>
        /// Το id του ζώου όπου αναφέρεται η συνομηλία
        /// </value>
        [BsonRepresentation(BsonType.ObjectId)]
        public String AnimalId { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
