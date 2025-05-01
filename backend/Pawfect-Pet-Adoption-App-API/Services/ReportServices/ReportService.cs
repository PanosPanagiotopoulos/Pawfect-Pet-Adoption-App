using AutoMapper;

using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.AdoptionApplication;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Report;
using Pawfect_Pet_Adoption_App_API.Query.Queries;
using Pawfect_Pet_Adoption_App_API.Repositories.Implementations;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;
using Pawfect_Pet_Adoption_App_API.Services.Convention;

namespace Pawfect_Pet_Adoption_App_API.Services.ReportServices
{
	public class ReportService : IReportService
	{
		private readonly ReportQuery _reportQuery;
		private readonly ReportBuilder _reportBuilder;
		private readonly IReportRepository _reportRepository;
		private readonly IMapper _mapper;
		private readonly IConventionService _conventionService;

		public ReportService
			(
				ReportQuery reportQuery,
				ReportBuilder reportBuilder,
				IReportRepository reportRepository,
				IMapper mapper,
				IConventionService conventionService
			)
		{
			_reportQuery = reportQuery;
			_reportBuilder = reportBuilder;
			_reportRepository = reportRepository;
			_mapper = mapper;
			_conventionService = conventionService;
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

		public async Task<ReportDto?> Persist(ReportPersist persist, List<String> fields)
		{
			Boolean isUpdate = _conventionService.IsValidId(persist.Id);
			Report data = new Report();
			String dataId = String.Empty;
			if (isUpdate)
			{
				data = await _reportRepository.FindAsync(x => x.Id == persist.Id);

				if (data == null) throw new InvalidDataException("No entity found with id given");

				_mapper.Map(persist, data);
				data.UpdatedAt = DateTime.UtcNow;
			}
			else
			{
				_mapper.Map(persist, data);
				data.Id = null; // Ensure new ID is generated
				data.CreatedAt = DateTime.UtcNow;
				data.UpdatedAt = DateTime.UtcNow;
			}

			if (isUpdate) dataId = await _reportRepository.UpdateAsync(data);
			else dataId = await _reportRepository.AddAsync(data);

			if (String.IsNullOrEmpty(dataId))
			{
				throw new InvalidOperationException("Αποτυχία κατά το persist της αναφοράς");
			}

			// Return dto model
			ReportLookup lookup = new ReportLookup(_reportQuery);
			lookup.Ids = new List<String> { dataId };
			lookup.Fields = fields;
			lookup.Offset = 0;
			lookup.PageSize = 1;

			return (
					 await _reportBuilder.SetLookup(lookup)
					.BuildDto(await lookup.EnrichLookup().CollectAsync(), lookup.Fields.ToList())
					).FirstOrDefault();
		}

		public async Task Delete(String id) { await this.Delete(new List<String>() { id }); }

		public async Task Delete(List<String> ids)
		{
			// TODO : Authorization
			await _reportRepository.DeleteAsync(ids);
		}
	}
}