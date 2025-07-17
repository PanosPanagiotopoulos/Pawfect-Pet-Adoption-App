using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Main_API.Builders;
using Main_API.Censors;
using Main_API.Data.Entities.Types.Authorization;
using Main_API.DevTools;
using Main_API.Exceptions;
using Main_API.Models.File;
using Main_API.Models.Lookups;
using Main_API.Query;
using Main_API.Services.AuthenticationServices;
using Main_API.Services.FileServices;
using Main_API.Transactions;
using System.Linq;
using System.Security.Claims;
using Pawfect_Pet_Adoption_App_API.Query;
using Main_API.Query.Queries;

namespace Main_API.Controllers
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
        private readonly IAuthorizationContentResolver _authorizationContentResolver;
        private readonly ClaimsExtractor _claimsExtractor;
        private readonly IQueryFactory _queryFactory;

        public FileController(
            IFileService fileService,
            ILogger<FileController> logger,
            IBuilderFactory builderFactory,
			ICensorFactory censorFactory,
            AuthContextBuilder contextBuilder,
            IAuthorizationContentResolver AuthorizationContentResolver,
            ClaimsExtractor claimsExtractor, 
            IQueryFactory queryFactory)
        {
            _fileService = fileService;
            _logger = logger;
            _builderFactory = builderFactory;
            _censorFactory = censorFactory;
            _contextBuilder = contextBuilder;
            _authorizationContentResolver = AuthorizationContentResolver;
            _claimsExtractor = claimsExtractor;
            _queryFactory = queryFactory;
        }

        /// <summary>
        /// Persist an animal.
        /// </summary>
        /// // TODO: How to handle case that someone spams files since not authorized? 
        [HttpPost("persist/temporary/many")]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> PersistBatchTemporarily()
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            IFormFileCollection files = Request.Form.Files;

            // Early validation for empty or null input
            if (files == null || files.Count == 0) return BadRequest("No files provided for upload.");

            IEnumerable<Models.File.FilePersist> filesPersisted = await _fileService.SaveTemporarily([..files]);

            return Ok(filesPersisted);
        }

        /// <summary>
        /// Persist an animal.
        /// </summary>
        [HttpPost("persist")]
        [Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> PersistBatch([FromBody] List<FilePersist> models, [FromQuery] List<String> fields)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            fields = BaseCensor.PrepareFieldsList(fields);

            List<Models.File.File>? files = await _fileService.Persist(models, fields);

            return Ok(files);
        }

        /// <summary>
        /// Query ζώων.
        /// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
        /// </summary>
        [HttpPost("query")]
		[Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> QueryFiles([FromBody] FileLookup fileLookup)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

            AuthContext context = _contextBuilder.OwnedFrom(fileLookup).AffiliatedWith(fileLookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<FileCensor>().Censor([.. fileLookup.Fields], context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying files");

            fileLookup.Fields = censoredFields;
            FileQuery q = fileLookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation);

            List<Data.Entities.File> datas = await q.CollectAsync();

            List<Models.File.File> models = await _builderFactory.Builder<FileBuilder>()
                .Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
                .Build(datas, [.. fileLookup.Fields]);

            if (models == null) throw new NotFoundException("Files not found", JsonHelper.SerializeObjectFormatted(fileLookup), typeof(Data.Entities.File));

            return Ok(new QueryResult<Models.File.File>()
            {
                Items = models,
                Count = await q.CountAsync()
            });
		}

		/// <summary>
		/// Λήψη ζώου με βάση το ID.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpGet("{id}")]
		[Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> GetFile(String id, [FromQuery] List<String> fields)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

            FileLookup lookup = new FileLookup
            {
                Offset = 1,
                PageSize = 1,
                Ids = new List<String> { id },
            };

            AuthContext context = _contextBuilder.OwnedFrom(lookup).AffiliatedWith(lookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<FileCensor>().Censor(BaseCensor.PrepareFieldsList([.. lookup.Fields]), context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying files");

            lookup.Fields = censoredFields;
            Models.File.File model = (await _builderFactory.Builder<FileBuilder>().Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
                .Build(await lookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation).CollectAsync(), censoredFields))
            .FirstOrDefault();

            if (model == null) throw new NotFoundException("Files not found", JsonHelper.SerializeObjectFormatted(lookup), typeof(Data.Entities.File));

            return Ok(model);
		}

		/// <summary>
		/// Delete a file by ID.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 404 NotFound, 500 String
		/// </summary>
		[HttpPost("delete")]
		[Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
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
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> DeleteMany([FromBody] List<String> ids)
		{
			// TODO: Add authorization
			if (ids == null || ids.Count == 0 || !ModelState.IsValid) return BadRequest(ModelState);

			await _fileService.Delete(ids);

			return Ok();
		}
	}
}
