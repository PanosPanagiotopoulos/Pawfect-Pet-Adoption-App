using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;

namespace Pawfect_Pet_Adoption_App_API.Models.Report
{
    public class ReportPersist
    {
        public string Id { get; set; }

        /// <value>
        /// Το Id του χρήστη που έκανε το report.
        /// </value>
        public string ReporterId { get; set; }

        /// <value>Το Id του χρήστη που έλαβε το report.</value>
        public string ReportedId { get; set; }

        /// <value>Ο τύπος αναφοράς βάση του συστήματος.</value>
        public ReportType Type { get; set; }

        public string Reason { get; set; }


        /// 
        /// <value>
        /// Η κατάσταση της αναφοράς
        /// </value>
        public ReportStatus Status { get; set; } = ReportStatus.Pending;
    }
}
