using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;

namespace Pawfect_Pet_Adoption_App_API.Models.Report
{
	public class ReportPersist
	{
		public String Id { get; set; }

		/// <value>
		/// Το Id του χρήστη που έκανε το report.
		/// </value>
		public String ReporterId { get; set; }

		/// <value>Το Id του χρήστη που έλαβε το report.</value>
		public String ReportedId { get; set; }

		/// <value>Ο τύπος αναφοράς βάση του συστήματος.</value>
		public ReportType Type { get; set; }

		public String Reason { get; set; }


		/// 
		/// <value>
		/// Η κατάσταση της αναφοράς
		/// </value>
		public ReportStatus Status { get; set; }
	}
}
