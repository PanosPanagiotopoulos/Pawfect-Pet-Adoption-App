using Pawfect_API.Models.Report;

namespace Pawfect_API.Services.ReportServices
{
	public interface IReportService
	{
		Task<Report?> Persist(ReportPersist persist, List<String> fields);
		Task Delete(String id);
		Task Delete(List<String> ids);
	}
}