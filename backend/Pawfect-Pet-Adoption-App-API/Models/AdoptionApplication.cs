using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Pawfect_Pet_Adoption_App_API.Models.EnumTypes;
using System.ComponentModel.DataAnnotations;

namespace Pawfect_Pet_Adoption_App_API.Models
{
    /// <summary>
    /// Το μοντέλο μιας αίτησης υιοθεσίας στο σύστημα
    /// </summary>
    public class AdoptionApplication
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [Required(ErrorMessage = "Το ID του χρήστη είναι απαραίτητο.")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }

        [Required(ErrorMessage = "Το ID του ζώου είναι απαραίτητο.")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string AnimalId { get; set; }

        [Required(ErrorMessage = "Το ID του καταφυγίου είναι απαραίτητο.")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ShelterId { get; set; }

        /// <value>
        /// Η κατάσταση του αιτήματος υιοθεσίας
        /// </value>
        [Required(ErrorMessage = "Η κατάσταση της αίτησης είναι απαραίτητη.")]
        [BsonRepresentation(BsonType.String)]
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

        /// <value>
        /// Γενικές πληροφορίες απο τον χρήστη για την υιοθεσία
        /// </value>

        [Required(ErrorMessage = "Πληροφορίες για την αίτηση είναι απαραίτητες.")]
        [MinLength(15, ErrorMessage = "Η περιγραφή της αίτησης υιοθεσίας πρέπει να έχει τουλάχιστον 15 χαρακτήρες.")]
        public string ApplicationDetails { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

}
