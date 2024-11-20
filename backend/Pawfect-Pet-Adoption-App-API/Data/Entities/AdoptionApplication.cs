using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;

namespace Pawfect_Pet_Adoption_App_API.Data.Entities
{
    /// <summary>
    /// Το μοντέλο μιας αίτησης υιοθεσίας στο σύστημα
    /// </summary>
    public class AdoptionApplication
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string AnimalId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string ShelterId { get; set; }

        /// <value>
        /// Η κατάσταση του αιτήματος υιοθεσίας
        /// </value>
        public ApplicationStatus Status { get; set; }

        /// <value>
        /// Γενικές πληροφορίες απο τον χρήστη για την υιοθεσία
        /// </value>

        public string ApplicationDetails { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
