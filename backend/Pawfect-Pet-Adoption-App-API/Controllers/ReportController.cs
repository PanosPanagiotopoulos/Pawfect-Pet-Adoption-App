using Microsoft.AspNetCore.Mvc;

using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Report;
using Pawfect_Pet_Adoption_App_API.Services.ReportServices;

namespace Pawfect_Pet_Adoption_App_API.Controllers
{
	[ApiController]
	[Route("api/reports")]
	public class ReportController : ControllerBase
	{
		private readonly IReportService _reportService;
		private readonly ILogger<ReportController> _logger;

		public ReportController(IReportService reportService, ILogger<ReportController> logger)
		{
			_reportService = reportService;
			_logger = logger;
		}

		/// <summary>
		/// Query reports.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpPost("query")]
		[ProducesResponseType(200, Type = typeof(IEnumerable<ReportDto>))]
		[ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
		[ProducesResponseType(404)]
		[ProducesResponseType(500, Type = typeof(String))]
		public async Task<IActionResult> QueryReports([FromBody] ReportLookup reportLookup)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				IEnumerable<ReportDto>? models = await _reportService.QueryReportsAsync(reportLookup);

				if (models == null)
				{
					return NotFound();
				}

				return Ok(models);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error καθώς κάναμε query reports");
				return RequestHandlerTool.HandleInternalServerError(e, "GET");
			}
		}

		/// <summary>
		/// Get report by ID.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpGet("{id}")]
		[ProducesResponseType(200, Type = typeof(ReportDto))]
		[ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
		[ProducesResponseType(404)]
		[ProducesResponseType(500, Type = typeof(String))]
		public async Task<IActionResult> GetReport(String id, [FromQuery] List<String> fields)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				ReportDto? model = await _reportService.Get(id, fields);

				if (model == null)
				{
					return NotFound();
				}

				return Ok(model);
			}
			catch (InvalidDataException e)
			{
				_logger.LogError(e, "Δεν βρέθηκε αναφορά");
				return NotFound();
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error καθώς κάναμε query report");
				return RequestHandlerTool.HandleInternalServerError(e, "GET");
			}
		}

		/// <summary>
		/// Persist a report.
		/// </summary>
		[HttpPost("persist")]
		[ProducesResponseType(200, Type = typeof(ReportDto))]
		[ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
		[ProducesResponseType(500, Type = typeof(String))]
		public async Task<IActionResult> Persist([FromBody] ReportPersist model, [FromQuery] List<String> fields)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				ReportDto? report = await _reportService.Persist(model, fields);

				if (report == null)
				{
					return RequestHandlerTool.HandleInternalServerError(new Exception("Failed to save model. Null return"), "POST");
				}

				return Ok(report);
			}
			catch (InvalidOperationException e)
			{
				_logger.LogError(e, "Αποτυχία αποθήκευσης αναφοράς");
				return NotFound();
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error ενώ κάναμε persist report");
				return RequestHandlerTool.HandleInternalServerError(e, "POST");
			}
		}

		/// <summary>
		/// Delete a report by ID.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 404 NotFound, 500 String
		/// </summary>
		[HttpPost("delete")]
		[ProducesResponseType(200)]
		[ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
		[ProducesResponseType(404)]
		[ProducesResponseType(500, Type = typeof(String))]
		public async Task<IActionResult> Delete([FromBody] String id)
		{
			// TODO: Add authorization
			if (String.IsNullOrEmpty(id) || !ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				await _reportService.Delete(id);
				return Ok();
			}
			catch (InvalidOperationException e)
			{
				_logger.LogError(e, "Αποτυχία διαγραφής αναφοράς με ID {Id}", id);
				return NotFound();
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error ενώ κάναμε delete report με ID {Id}", id);
				return RequestHandlerTool.HandleInternalServerError(e, "POST");
			}
		}

		/// <summary>
		/// Delete multiple reports by IDs.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 404 NotFound, 500 String
		/// </summary>
		[HttpPost("delete/many")]
		[ProducesResponseType(200)]
		[ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
		[ProducesResponseType(404)]
		[ProducesResponseType(500, Type = typeof(String))]
		public async Task<IActionResult> DeleteMany([FromBody] List<String> ids)
		{
			// TODO: Add authorization
			if (ids == null || !ids.Any() || !ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				await _reportService.Delete(ids);
				return Ok();
			}
			catch (InvalidOperationException e)
			{
				_logger.LogError(e, "Αποτυχία διαγραφής αναφορών με IDs {Ids}", String.Join(", ", ids));
				return NotFound();
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error ενώ κάναμε delete πολλαπλών reports με IDs {Ids}", String.Join(", ", ids));
				return RequestHandlerTool.HandleInternalServerError(e, "POST");
			}
		}
	}
}
