using Microsoft.AspNetCore.Mvc;

using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Shelter;
using Pawfect_Pet_Adoption_App_API.Services.ShelterServices;

namespace Pawfect_Pet_Adoption_App_API.Controllers
{
	[ApiController]
	[Route("api/shelters")]
	public class ShelterController : ControllerBase
	{
		private readonly IShelterService _shelterService;
		private readonly ILogger<ShelterController> _logger;

		public ShelterController(IShelterService shelterService, ILogger<ShelterController> logger)
		{
			_shelterService = shelterService;
			_logger = logger;
		}

		/// <summary>
		/// Query ζώων.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpPost("query")]
		[ProducesResponseType(200, Type = typeof(IEnumerable<ShelterDto>))]
		[ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
		[ProducesResponseType(404)]
		[ProducesResponseType(500, Type = typeof(String))]
		public async Task<IActionResult> QueryShelters([FromBody] ShelterLookup shelterLookup)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				IEnumerable<ShelterDto>? models = await _shelterService.QuerySheltersAsync(shelterLookup);

				if (models == null)
				{
					return NotFound();
				}

				return Ok(models);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error καθώς κάναμε query shelters");
				return RequestHandlerTool.HandleInternalServerError(e, "GET");
			}
		}

		/// <summary>
		/// Λήψη ζώου με βάση το ID.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpGet("{id}")]
		[ProducesResponseType(200, Type = typeof(ShelterDto))]
		[ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
		[ProducesResponseType(404)]
		[ProducesResponseType(500, Type = typeof(String))]
		public async Task<IActionResult> GetShelter(String id, [FromQuery] List<String> fields)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				ShelterDto? model = await _shelterService.Get(id, fields);

				if (model == null)
				{
					return NotFound();
				}

				return Ok(model);
			}
			catch (InvalidDataException e)
			{
				_logger.LogError(e, "Δεν βρέθηκε το καταφύγιο");
				return NotFound();
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error καθώς κάναμε query shelter");
				return RequestHandlerTool.HandleInternalServerError(e, "GET");
			}
		}

		/// <summary>
		/// Persist an shelter.
		/// </summary>
		[HttpPost("persist")]
		[ProducesResponseType(200, Type = typeof(ShelterDto))]
		[ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
		[ProducesResponseType(500, Type = typeof(String))]
		public async Task<IActionResult> Persist([FromBody] ShelterPersist model)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				ShelterDto? shelter = await _shelterService.Persist(model);

				if (shelter == null)
				{
					return RequestHandlerTool.HandleInternalServerError(new Exception("Failed to save model. Null return"), "POST");
				}

				return Ok(shelter);
			}
			catch (InvalidOperationException e)
			{
				_logger.LogError(e, "Αποτυχία αποθήκευσης shelter");
				return NotFound();
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error ενώ κάναμε persist shelter");
				return RequestHandlerTool.HandleInternalServerError(e, "POST");
			}
		}
	}
}
