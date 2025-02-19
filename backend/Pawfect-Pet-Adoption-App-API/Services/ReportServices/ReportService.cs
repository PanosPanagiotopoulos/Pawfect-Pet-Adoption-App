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

		public async Task<ReportDto?> Get(String id, List<String> fields)
		{
			ReportLookup lookup = new ReportLookup(_reportQuery);
			lookup.Ids = new List<String> { id };
			lookup.Fields = fields;
			lookup.PageSize = 1;
			lookup.Offset = 0;

			List<Report> report = await lookup.EnrichLookup().CollectAsync();

			if (report == null)
			{
				throw new InvalidDataException("Δεν βρέθηκε αναφορά με αυτό το ID");
			}

			return (await _reportBuilder.SetLookup(lookup).BuildDto(report, fields)).FirstOrDefault();
		}
	}
}