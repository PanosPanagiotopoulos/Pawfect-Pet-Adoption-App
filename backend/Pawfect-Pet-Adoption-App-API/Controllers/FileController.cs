using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Censors;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Exceptions;
using Pawfect_Pet_Adoption_App_API.Models.File;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Query;
using Pawfect_Pet_Adoption_App_API.Services.FileServices;
using System.Reflection;

namespace Pawfect_Pet_Adoption_App_API.Controllers
{
	[ApiController]
	[Route("api/files")]
	public class FileController: ControllerBase
	{
        private readonly IFileService _fileService;
        private readonly ILogger<FileController> _logger;
        private readonly IBuilderFactory _builderFactory;
        private readonly ICensorFactory _censorFactory;
        private readonly AuthContextBuilder _contextBuilder;
        private readonly IQueryFactory _queryFactory;

        public FileController(
            IFileService fileService,
            ILogger<FileController> logger,
            IBuilderFactory builderFactory,
			ICensorFactory censorFactory,
            AuthContextBuilder contextBuilder,
            IQueryFactory queryFactory)
        {
            _fileService = fileService;
            _logger = logger;
            _builderFactory = builderFactory;
            _censorFactory = censorFactory;
            _contextBuilder = contextBuilder;
            _queryFactory = queryFactory;
        }

        /// <summary>
        /// Query ζώων.
        /// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
        /// </summary>
        [HttpPost("query")]
		[Authorize]
		public async Task<IActionResult> QueryFiles([FromBody] FileLookup fileLookup)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

            AuthContext context = _contextBuilder.OwnedFrom(fileLookup).AffiliatedWith(fileLookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<FileCensor>().Censor([.. fileLookup.Fields], context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying files");

            fileLookup.Fields = censoredFields;
            List<Data.Entities.File> datas = await fileLookup
                .EnrichLookup(_queryFactory)
                .Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
                .CollectAsync();

            List<Models.File.File> models = await _builderFactory.Builder<FileBuilder>()
                .Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
                .Build(datas, [.. fileLookup.Fields]);

            if (models == null) throw new NotFoundException("Files not found", JsonHelper.SerializeObjectFormatted(fileLookup), typeof(Data.Entities.File));

            return Ok(models);
		}

		/// <summary>
		/// Λήψη ζώου με βάση το ID.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpGet("{id}")]
		[Authorize]
		public async Task<IActionResult> GetFile(String id, [FromQuery] List<String> fields)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

            FileLookup lookup = new FileLookup
            {
                Offset = 1,
                PageSize = 1,
                Ids = new List<String> { id },
                Fields = fields
            };

            AuthContext context = _contextBuilder.OwnedFrom(lookup).AffiliatedWith(lookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<FileCensor>().Censor([.. lookup.Fields], context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying files");

            lookup.Fields = censoredFields;
            Models.File.File model = (await _builderFactory.Builder<FileBuilder>().Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
                .Build(await lookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation).CollectAsync(), censoredFields))
            .FirstOrDefault();

            if (model == null) throw new NotFoundException("Files not found", JsonHelper.SerializeObjectFormatted(lookup), typeof(Data.Entities.File));

            return Ok(model);
		}

		/// <summary>
		/// Persist an animal.
		/// </summary>
		[HttpPost("persist/temporary")]
		[Authorize]
		public async Task<IActionResult> PersistBatchTemporarily([FromForm] List<TempMediaFile> models)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			// Early validation for empty or null input
			if (models == null || models.Count == 0) return BadRequest("No files provided for upload.");

            IEnumerable<Models.File.File>? files = await _fileService.SaveTemporarily(models);

			return Ok(files);
		}
			

		/// <summary>
		/// Persist an animal.
		/// </summary>
		[HttpPost("persist")]
		[Authorize]
        public async Task<IActionResult> PersistBatch([FromBody] List<FilePersist> models, [FromQuery] List<String> fields)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

            List<Models.File.File>? files = await _fileService.Persist(models, fields);

			return Ok(files);
		}

		/// <summary>
		/// Delete a file by ID.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 404 NotFound, 500 String
		/// </summary>
		[HttpPost("delete")]
		[Authorize]
		public async Task<IActionResult> Delete([FromBody] String id)
		{
			if (String.IsNullOrEmpty(id) || !ModelState.IsValid) return BadRequest(ModelState);

			await _fileService.Delete(id);

			return Ok();
		}

		/// <summary>
		/// Delete multiple files by IDs.
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
			if (ids == null || ids.Count == 0 || !ModelState.IsValid) return BadRequest(ModelState);

			await _fileService.Delete(ids);

			return Ok();
		}
	}
}
