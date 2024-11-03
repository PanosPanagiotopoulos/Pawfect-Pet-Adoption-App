using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Pawfect_Pet_Adoption_App_API.Models
{
    /// <summary>
    /// Το μοντέλο ενός μηνύματος στο σύστημα
    /// </summary>
    public class Message
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [Required(ErrorMessage = "Το ID της συνομιλίας είναι απαραίτητο.")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ConversationId { get; set; }

        [Required(ErrorMessage = "Το ID του αποστολέα είναι απαραίτητο.")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string SenderId { get; set; }

        [Required(ErrorMessage = "Το ID του παραλήπτη είναι απαραίτητο.")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string RecepientId { get; set; }

        [Required(ErrorMessage = "Το περιεχόμενο του μηνύματος είναι απαραίτητο.")]
        public string Content { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
