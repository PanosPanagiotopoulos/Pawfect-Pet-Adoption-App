using Main_API.Data.Entities.EnumTypes;
using Main_API.Models.User;

namespace Main_API.Models.Report
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
