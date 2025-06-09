using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
using Pawfect_Pet_Adoption_App_API.Models.User;

namespace Pawfect_Pet_Adoption_App_API.Models.Report
{
	public class Report
	{
		public String? Id { get; set; }

		/// <value>
		/// Το Id του χρήστη που έκανε το report.
		/// </value>
		public User.User? Reporter { get; set; }

		public User.User? Reported { get; set; }

		/// <value>Ο τύπος αναφοράς βάση του συστήματος.</value>
		public ReportType? Type { get; set; }

		public String? Reason { get; set; }


		/// 
		/// <value>
		/// Η κατάσταση της αναφοράς
		/// </value>
		public ReportStatus? Status { get; set; }

		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }

	}
}
