using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Report;

namespace Pawfect_Pet_Adoption_App_API.Services
{
    public interface IReportService
    {
        // Συνάρτηση για query στα reports
        Task<IEnumerable<ReportDto>> QueryReportsAsync(ReportLookup reportLookup);
    }
}