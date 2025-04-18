﻿using Microsoft.AspNetCore.Mvc;

using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Models.Animal;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Services.AnimalServices;

namespace Pawfect_Pet_Adoption_App_API.Controllers
{
	[ApiController]
	[Route("api/animals")]
	public class AnimalController : ControllerBase
	{
		private readonly IAnimalService _animalService;
		private readonly ILogger<AnimalController> _logger;

		public AnimalController(IAnimalService animalService, ILogger<AnimalController> logger)
		{
			_animalService = animalService;
			_logger = logger;
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
				IEnumerable<AnimalDto>? models = await _animalService.QueryAnimalsAsync(animalLookup);

				if (models == null)
				{
					return NotFound();
				}

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
				AnimalDto? model = await _animalService.Get(id, fields);

				if (model == null)
				{
					return NotFound();
				}

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
		public async Task<IActionResult> Persist([FromBody] AnimalPersist model)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				AnimalDto? animal = await _animalService.Persist(model);

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
	}
}
