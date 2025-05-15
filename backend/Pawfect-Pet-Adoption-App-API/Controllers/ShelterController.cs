using Microsoft.AspNetCore.Mvc;
using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Models.AdoptionApplication;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Shelter;
using Pawfect_Pet_Adoption_App_API.Query;
using Pawfect_Pet_Adoption_App_API.Services.ShelterServices;

namespace Pawfect_Pet_Adoption_App_API.Controllers
{
	[ApiController]
	[Route("api/shelters")]
	public class ShelterController : ControllerBase
	{
		private readonly IShelterService _shelterService;
		private readonly ILogger<ShelterController> _logger;
        private readonly IQueryFactory _queryFactory;
        private readonly IBuilderFactory _builderFactory;

        public ShelterController
			(
				IShelterService shelterService, ILogger<ShelterController> logger,
				IQueryFactory queryFactory, IBuilderFactory builderFactory
            )
		{
			_shelterService = shelterService;
			_logger = logger;
            _queryFactory = queryFactory;
            _builderFactory = builderFactory;
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
                List<Data.Entities.Shelter> datas = await shelterLookup
                                                        .EnrichLookup(_queryFactory)
                                                        .Authorise(Data.Entities.Types.Authorisation.AuthorizationFlags.OwnerOrPermissionOrAffiliation)
                                                        .CollectAsync();

                List<ShelterDto> models = await _builderFactory.Builder<ShelterBuilder>()
                                                    .Authorise(Data.Entities.Types.Authorisation.AuthorizationFlags.OwnerOrPermissionOrAffiliation)
                                                    .BuildDto(datas, shelterLookup.Fields.ToList());

                if (models == null) return NotFound();

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
                ShelterLookup lookup = new ShelterLookup();

                // Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
                lookup.Offset = 1;
                // Γενική τιμή για τη λήψη των dtos
                lookup.PageSize = 1;
                lookup.Ids = [id];
                lookup.Fields = fields;

                ShelterDto model = (await _builderFactory.Builder<ShelterBuilder>().BuildDto(await lookup.EnrichLookup(_queryFactory).CollectAsync(), fields)).FirstOrDefault();

                if (model == null) return NotFound();


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

		/// <summary>
		/// Delete a shelter by ID.
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
				await _shelterService.Delete(id);
				return Ok();
			}
			catch (InvalidOperationException e)
			{
				_logger.LogError(e, "Αποτυχία διαγραφής καταφυγίου με ID {Id}", id);
				return NotFound();
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error ενώ κάναμε delete καταφυγίου με ID {Id}", id);
				return RequestHandlerTool.HandleInternalServerError(e, "POST");
			}
		}

		/// <summary>
		/// Delete multiple shelters by IDs.
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
				await _shelterService.Delete(ids);
				return Ok();
			}
			catch (InvalidOperationException e)
			{
				_logger.LogError(e, "Αποτυχία διαγραφής καταφυγίων με IDs {Ids}", String.Join(", ", ids));
				return NotFound();
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error ενώ κάναμε delete πολλαπλών καταφυγίων με IDs {Ids}", String.Join(", ", ids));
				return RequestHandlerTool.HandleInternalServerError(e, "POST");
			}
		}
	}
}
