using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Censors;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Exceptions;
using Pawfect_Pet_Adoption_App_API.Models.AdoptionApplication;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Report;
using Pawfect_Pet_Adoption_App_API.Query;
using Pawfect_Pet_Adoption_App_API.Services.ReportServices;
using System.Reflection;

namespace Pawfect_Pet_Adoption_App_API.Controllers
{
	[ApiController]
	[Route("api/reports")]
	public class ReportController : ControllerBase
	{
		private readonly IReportService _reportService;
		private readonly ILogger<ReportController> _logger;
        private readonly IQueryFactory _queryFactory;
        private readonly IBuilderFactory _builderFactory;
        private readonly ICensorFactory _censorFactory;
        private readonly AuthContextBuilder _contextBuilder;

        public ReportController(
				IReportService reportService, ILogger<ReportController> logger,
				IQueryFactory queryFactory, IBuilderFactory builderFactory,
                ICensorFactory censorFactory, AuthContextBuilder contextBuilder
            )
		{
			_reportService = reportService;
			_logger = logger;
            _queryFactory = queryFactory;
            _builderFactory = builderFactory;
            _censorFactory = censorFactory;
            _contextBuilder = contextBuilder;
        }

		/// <summary>
		/// Query reports.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpPost("query")]
		[ProducesResponseType(200, Type = typeof(IEnumerable<Report>))]
		[ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
		[ProducesResponseType(404)]
		[ProducesResponseType(500, Type = typeof(String))]
		public async Task<IActionResult> QueryReports([FromBody] ReportLookup reportLookup)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

            AuthContext context = _contextBuilder.OwnedFrom(reportLookup).AffiliatedWith(reportLookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<ReportCensor>().Censor([.. reportLookup.Fields], context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying reports");

            reportLookup.Fields = censoredFields;
            List<Data.Entities.Report> datas = await reportLookup
                                                    .EnrichLookup(_queryFactory)
                                                    .Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
                                                    .CollectAsync();

            List<Report> models = await _builderFactory.Builder<ReportBuilder>()
                                                .Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
                                                .Build(datas, [.. reportLookup.Fields]);

            if (models == null) throw new NotFoundException("Reports not found", JsonHelper.SerializeObjectFormatted(reportLookup), typeof(Data.Entities.Report));

            return Ok(models);
		}

		/// <summary>
		/// Get report by ID.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpGet("{id}")]
		[Authorize]
		public async Task<IActionResult> GetReport(String id, [FromQuery] List<String> fields)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

            ReportLookup lookup = new ReportLookup();

            // Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
            lookup.Offset = 1;
            // Γενική τιμή για τη λήψη των dtos
            lookup.PageSize = 1;
            lookup.Ids = [id];
            lookup.Fields = fields;

            AuthContext context = _contextBuilder.OwnedFrom(lookup).AffiliatedWith(lookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<ReportCensor>().Censor([.. lookup.Fields], context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying reports");

            lookup.Fields = censoredFields;
            Report model = (await _builderFactory.Builder<ReportBuilder>().Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
													.Build(await lookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation).CollectAsync(), censoredFields))
            .FirstOrDefault();

            if (model == null) throw new NotFoundException("Report not found", JsonHelper.SerializeObjectFormatted(lookup), typeof(Data.Entities.Report));


            return Ok(model);
		}

		/// <summary>
		/// Persist a report.
		/// </summary>
		[HttpPost("persist")]
		[Authorize]
		public async Task<IActionResult> Persist([FromBody] ReportPersist model, [FromQuery] List<String> fields)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			Report? report = await _reportService.Persist(model, fields);

			return Ok(report);
		}

		/// <summary>
		/// Delete a report by ID.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 404 NotFound, 500 String
		/// </summary>
		[HttpPost("delete")]
		[Authorize]
		public async Task<IActionResult> Delete([FromBody] String id)
		{
			if (String.IsNullOrEmpty(id) || !ModelState.IsValid) return BadRequest(ModelState);

			await _reportService.Delete(id);

			return Ok();
		}

		/// <summary>
		/// Delete multiple reports by IDs.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 404 NotFound, 500 String
		/// </summary>
		[HttpPost("delete/many")]
		[Authorize]
		public async Task<IActionResult> DeleteMany([FromBody] List<String> ids)
		{
			if (ids == null || !ids.Any() || !ModelState.IsValid) return BadRequest(ModelState);

			await _reportService.Delete(ids);

			return Ok();
		}
	}
}
