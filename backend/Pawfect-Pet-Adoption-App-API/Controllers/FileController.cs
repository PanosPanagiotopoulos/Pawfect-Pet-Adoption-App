using Microsoft.AspNetCore.Mvc;
using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Models.File;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Services.FileServices;

namespace Pawfect_Pet_Adoption_App_API.Controllers
{
	[ApiController]
	[Route("api/files")]
	public class FileController: ControllerBase
	{
		private readonly IFileService _fileService;
		private readonly ILogger<FileController> _logger;

		public FileController(IFileService fileService, ILogger<FileController> logger)
		{
			_fileService = fileService;
			_logger = logger;
		}

		/// <summary>
		/// Query ζώων.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpPost("query")]
		[ProducesResponseType(200, Type = typeof(IEnumerable<FileDto>))]
		[ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
		[ProducesResponseType(404)]
		[ProducesResponseType(500, Type = typeof(String))]
		public async Task<IActionResult> QueryAnimals([FromBody] FileLookup fileLookup)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				IEnumerable<FileDto>? models = await _fileService.QueryFilesAsync(fileLookup);

				if (models == null)
				{
					return NotFound();
				}

				return Ok(models);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error καθώς κάναμε query files");
				return RequestHandlerTool.HandleInternalServerError(e, "GET");
			}
		}

		/// <summary>
		/// Λήψη ζώου με βάση το ID.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpGet("{id}")]
		[ProducesResponseType(200, Type = typeof(FileDto))]
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
				FileDto? model = await _fileService.Get(id, fields);

				if (model == null)
				{
					return NotFound();
				}

				return Ok(model);
			}
			catch (InvalidDataException e)
			{
				_logger.LogError(e, "Δεν βρέθηκε file");
				return NotFound();
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error καθώς κάναμε query files");
				return RequestHandlerTool.HandleInternalServerError(e, "GET");
			}
		}

		/// <summary>
		/// Persist an animal.
		/// </summary>
		[HttpPost("persist/temporary")]
		[ProducesResponseType(200, Type = typeof(List<FileDto>))]
		[ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
		[ProducesResponseType(500, Type = typeof(String))]
		public async Task<IActionResult> PersistBatchTemporarily([FromForm] List<TempMediaFile> models)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			// Early validation for empty or null input
			if (models == null || !models.Any())
			{
				return BadRequest("No files provided for upload.");
			}

			try
			{
				IEnumerable<FileDto>? files = await _fileService.SaveTemporarily(models);

				if (files == null)
				{
					return RequestHandlerTool.HandleInternalServerError(new Exception("Failed to save model. Null return"), "POST");
				}

				return Ok(files);
			}
			catch (InvalidOperationException e)
			{
				_logger.LogError(e, "Αποτυχία αποθήκευσης files");
				return NotFound();
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error ενώ κάναμε persist files");
				return RequestHandlerTool.HandleInternalServerError(e, "POST");
			}
		}

		/// <summary>
		/// Persist an animal.
		/// </summary>
		[HttpPost("persist")]
		[ProducesResponseType(200, Type = typeof(List<FileDto>))]
		[ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
		[ProducesResponseType(500, Type = typeof(String))]
		public async Task<IActionResult> PersistBatch([FromBody] List<FilePersist> models)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				List<FileDto>? files = await _fileService.Persist(models);

				if (files == null)
				{
					return RequestHandlerTool.HandleInternalServerError(new Exception("Failed to save model. Null return"), "POST");
				}

				return Ok(files);
			}
			catch (InvalidOperationException e)
			{
				_logger.LogError(e, "Αποτυχία αποθήκευσης files");
				return NotFound();
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error ενώ κάναμε persist files");
				return RequestHandlerTool.HandleInternalServerError(e, "POST");
			}
		}
	}
}
