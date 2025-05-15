using Microsoft.AspNetCore.Mvc;
using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Models.Animal;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Query;
using Pawfect_Pet_Adoption_App_API.Services.AnimalServices;

namespace Pawfect_Pet_Adoption_App_API.Controllers
{
	[ApiController]
	[Route("api/animals")]
	public class AnimalController : ControllerBase
	{
        private readonly IAnimalService _animalService;
        private readonly ILogger<AnimalController> _logger;
        private readonly IBuilderFactory _builderFactory;
        private readonly IQueryFactory _queryFactory;

        public AnimalController(
            IAnimalService animalService,
            ILogger<AnimalController> logger,
            IBuilderFactory builderFactory,
            IQueryFactory queryFactory)
        {
            _animalService = animalService;
            _logger = logger;
            _builderFactory = builderFactory;
            _queryFactory = queryFactory;
        }

        /// <summary>
        /// Query ζώων.
        /// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
        /// </summary>
        [HttpPost("query")]
		[ProducesResponseType(200, Type = typeof(IEnumerable<AnimalDto>))]
		[ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
		[ProducesResponseType(404)]
		[ProducesResponseType(500, Type = typeof(String))]
		public async Task<IActionResult> QueryAnimals([FromBody] AnimalLookup animalLookup)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

            try
            {
                List<Data.Entities.Animal> datas = await animalLookup
                    .EnrichLookup(_queryFactory)
                    .Authorise(Data.Entities.Types.Authorisation.AuthorizationFlags.OwnerOrPermissionOrAffiliation)
                    .CollectAsync();

                List<AnimalDto> models = await _builderFactory.Builder<AnimalBuilder>()
                    .Authorise(Data.Entities.Types.Authorisation.AuthorizationFlags.OwnerOrPermissionOrAffiliation)
                    .BuildDto(datas, animalLookup.Fields.ToList());

                if (models == null) return NotFound();

                return Ok(models);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error καθώς κάναμε query animals");
                return RequestHandlerTool.HandleInternalServerError(e, "GET");
            }
        }

		/// <summary>
		/// Λήψη ζώου με βάση το ID.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpGet("{id}")]
		[ProducesResponseType(200, Type = typeof(AnimalDto))]
		[ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
		[ProducesResponseType(404)]
		[ProducesResponseType(500, Type = typeof(String))]
		public async Task<IActionResult> GetAnimal(String id, [FromQuery] List<String> fields)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
                AnimalLookup lookup = new AnimalLookup
                {
                    Offset = 1,
                    PageSize = 1,
                    Ids = new List<String> { id },
                    Fields = fields
                };

                AnimalDto model = (await _builderFactory.Builder<AnimalBuilder>()
                    .BuildDto(await lookup.EnrichLookup(_queryFactory).CollectAsync(), fields))
                    .FirstOrDefault();

                if (model == null) return NotFound();

                return Ok(model);
            }
            catch (InvalidDataException e)
			{
				_logger.LogError(e, "Δεν βρέθηκε ζώο");
				return NotFound();
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error καθώς κάναμε query animal");
				return RequestHandlerTool.HandleInternalServerError(e, "GET");
			}
		}

		/// <summary>
		/// Persist an animal.
		/// </summary>
		[HttpPost("persist")]
		[ProducesResponseType(200, Type = typeof(AnimalDto))]
		[ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
		[ProducesResponseType(500, Type = typeof(String))]
		public async Task<IActionResult> Persist([FromBody] AnimalPersist model, [FromQuery] List<String> fields)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				AnimalDto? animal = await _animalService.Persist(model, fields);

				if (animal == null)
				{
					return RequestHandlerTool.HandleInternalServerError(new Exception("Failed to save model. Null return"), "POST");
				}

				return Ok(animal);
			}
			catch (InvalidOperationException e)
			{
				_logger.LogError(e, "Αποτυχία αποθήκευσης ζώου");
				return NotFound();
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error ενώ κάναμε persist animal");
				return RequestHandlerTool.HandleInternalServerError(e, "POST");
			}
		}

		/// <summary>
		/// Delete an animal by ID.
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
				await _animalService.Delete(id);
				return Ok();
			}
			catch (InvalidOperationException e)
			{
				_logger.LogError(e, "Αποτυχία διαγραφής ζώου με ID {Id}", id);
				return NotFound();
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error ενώ κάναμε delete animal με ID {Id}", id);
				return RequestHandlerTool.HandleInternalServerError(e, "POST");
			}
		}

		/// <summary>
		/// Delete multiple animals by IDs.
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
				await _animalService.Delete(ids);
				return Ok();
			}
			catch (InvalidOperationException e)
			{
				_logger.LogError(e, "Αποτυχία διαγραφής ζώων με IDs {Ids}", String.Join(", ", ids));
				return NotFound();
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error ενώ κάναμε delete πολλαπλών animals με IDs {Ids}", String.Join(", ", ids));
				return RequestHandlerTool.HandleInternalServerError(e, "POST");
			}
		}
	}
}
