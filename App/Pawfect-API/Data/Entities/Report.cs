using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Pawfect_API.Data.Entities.EnumTypes;

namespace Pawfect_API.Data.Entities
{
    /// <summary>
    /// Το μοντέλο μιας αναφοράς στο σύστημα
    /// </summary>
    public class Report
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public String Id { get; set; }

        /// <value>
        /// Το Id του χρήστη που έκανε το report.
        /// </value>
        [BsonRepresentation(BsonType.ObjectId)]
        public String ReporterId { get; set; }

        /// <value>Το Id του χρήστη που έλαβε το report.</value>
        [BsonRepresentation(BsonType.ObjectId)]
        public String ReportedId { get; set; }

        /// <value>Ο τύπος αναφοράς βάση του συστήματος.</value>
        public ReportType Type { get; set; }

        public String Reason { get; set; }


        /// 
        /// <value>
        /// Η κατάσταση της αναφοράς
        /// </value>
        public ReportStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

    }
}
