using Microsoft.AspNetCore.Mvc;

using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Models.AdoptionApplication;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Services.AdoptionApplicationServices;

namespace Pawfect_Pet_Adoption_App_API.Controllers
{
	[ApiController]
	[Route("api/adoptionApplication")]
	public class AdoptionApplicationController : ControllerBase
	{
		private readonly IAdoptionApplicationService _adoptionApplicationService;
		private readonly ILogger<AdoptionApplicationController> _logger;

		public AdoptionApplicationController(IAdoptionApplicationService adoptionApplicationService, ILogger<AdoptionApplicationController> logger)
		{
			_adoptionApplicationService = adoptionApplicationService;
			_logger = logger;
		}

		/// <summary>
		/// Query adoptionApplications.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpGet("query")]
		[ProducesResponseType(200, Type = typeof(IEnumerable<AdoptionApplicationDto>))]
		[ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
		[ProducesResponseType(404)]
		[ProducesResponseType(500, Type = typeof(String))]
		public async Task<IActionResult> QueryAdoptionApplications([FromQuery] AdoptionApplicationLookup adoptionApplicationLookup)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				IEnumerable<AdoptionApplicationDto>? adoptionApplications = await _adoptionApplicationService.QueryAdoptionApplicationsAsync(adoptionApplicationLookup);

				if (adoptionApplications == null)
				{
					return NotFound();
				}

				return Ok(adoptionApplications);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error ενώ κάναμε query adoptionApplications");
				return RequestHandlerTool.HandleInternalServerError(e, "GET");
			}
		}

		/// <summary>
		/// Get adoptionApplication by ID.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpGet("{id}")]
		[ProducesResponseType(200, Type = typeof(AdoptionApplicationDto))]
		[ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
		[ProducesResponseType(404)]
		[ProducesResponseType(500, Type = typeof(String))]
		public async Task<IActionResult> GetAdoptionApplication([FromRoute] String id, [FromQuery] List<String> fields)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				AdoptionApplicationDto? adoptionApplication = await _adoptionApplicationService.Get(id, fields);

				if (adoptionApplication == null)
				{
					return NotFound();
				}

				return Ok(adoptionApplication);
			}
			catch (InvalidDataException e)
			{
				_logger.LogError(e, "Δεν βρέθηκε ειδοποίηση");
				return NotFound();
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error ενώ κάναμε query adoptionApplication");
				return RequestHandlerTool.HandleInternalServerError(e, "GET");
			}
		}
	}
}
