using Pawfect_Pet_Adoption_App_API.Models.Report;

namespace Pawfect_Pet_Adoption_App_API.Services.ReportServices
{
	public interface IReportService
	{
		Task<ReportDto?> Persist(ReportPersist persist, List<String> fields);
		Task Delete(String id);
		Task Delete(List<String> ids);
	}
}