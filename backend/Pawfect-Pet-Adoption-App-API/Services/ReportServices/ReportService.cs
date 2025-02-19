using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Report;
using Pawfect_Pet_Adoption_App_API.Query.Queries;

namespace Pawfect_Pet_Adoption_App_API.Services.ReportServices
{
	public class ReportService : IReportService
	{
		private readonly ReportQuery _reportQuery;
		private readonly ReportBuilder _reportBuilder;

		public ReportService(ReportQuery reportQuery, ReportBuilder reportBuilder)
		{
			_reportQuery = reportQuery;
			_reportBuilder = reportBuilder;
		}

		public async Task<IEnumerable<ReportDto>> QueryReportsAsync(ReportLookup reportLookup)
		{
			List<Report> queriedReports = await reportLookup.EnrichLookup(_reportQuery).CollectAsync();
			return await _reportBuilder.SetLookup(reportLookup).BuildDto(queriedReports, reportLookup.Fields.ToList());
		}
	}
}