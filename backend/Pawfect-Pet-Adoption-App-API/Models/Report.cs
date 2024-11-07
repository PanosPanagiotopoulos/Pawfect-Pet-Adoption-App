using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Pawfect_Pet_Adoption_App_API.Models.EnumTypes;
using System.ComponentModel.DataAnnotations;

namespace Pawfect_Pet_Adoption_App_API.Models
{
    /// <summary>
    /// Το μοντέλο μιας αναφοράς στο σύστημα
    /// </summary>
    public class Report
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        /// <value>
        /// Το Id του χρήστη που έκανε το report.
        /// </value>
        [Required(ErrorMessage = "Το ID του χρήστη που έκανε την αναφορά είναι απαραίτητο.")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ReporterId { get; set; }

        /// <value>Το Id του χρήστη που έλαβε το report.</value>
        [Required(ErrorMessage = "Το ID του χρήστη που έλαβε την αναφορά είναι απαραίτητο.")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ReportedId { get; set; }

        /// <value>Ο τύπος αναφοράς βάση του συστήματος.</value>
        [Required(ErrorMessage = "Ο τύπος της αναφοράς είναι απαραίτητος.")]
        [BsonRepresentation(BsonType.String)]
        public ReportType Type { get; set; }

        [Required(ErrorMessage = "Η περιγραφή της αναφοράς είναι απαραίτητη.")]
        public string Reason { get; set; }


        /// 
        /// <value>
        /// Η κατάσταση της αναφοράς
        /// </value>
        [BsonRepresentation(BsonType.String)]
        public ReportStatus Status { get; set; } = ReportStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
