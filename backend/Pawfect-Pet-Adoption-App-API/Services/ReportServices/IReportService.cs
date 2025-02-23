using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Report;

namespace Pawfect_Pet_Adoption_App_API.Services.ReportServices
{
	public interface IReportService
	{
		// Συνάρτηση για query στα reports
		Task<IEnumerable<ReportDto>> QueryReportsAsync(ReportLookup reportLookup);

		Task<ReportDto?> Get(String id, List<String> fields);

		Task<ReportDto?> Persist(ReportPersist persist);
	}
}