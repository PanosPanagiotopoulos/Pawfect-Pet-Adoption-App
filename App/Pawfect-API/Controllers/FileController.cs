using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawfect_API.Builders;
using Pawfect_API.Censors;
using Pawfect_API.Data.Entities.Types.Authorization;
using Pawfect_API.DevTools;
using Pawfect_API.Exceptions;
using Pawfect_API.Models.Lookups;
using Pawfect_API.Query;
using Pawfect_API.Services.AuthenticationServices;
using Pawfect_API.Services.FileServices;
using Pawfect_API.Transactions;
using Pawfect_Pet_Adoption_App_API.Query;
using Pawfect_API.Query.Queries;
using Pawfect_Pet_Adoption_App_API.Attributes;
using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;

namespace Pawfect_API.Controllers
{
	[ApiController]
	[Route("api/files")]
    [RateLimit(RateLimitLevel.Moderate)]
    public class FileController: ControllerBase
	{
        private readonly IFileService _fileService;
        private readonly ILogger<FileController> _logger;
        private readonly IBuilderFactory _builderFactory;
        private readonly ICensorFactory _censorFactory;
        private readonly AuthContextBuilder _contextBuilder;
        private readonly IAuthorizationContentResolver _authorizationContentResolver;
        private readonly ClaimsExtractor _claimsExtractor;
        private readonly IFileAccessService _accessService;
        private readonly IQueryFactory _queryFactory;

        public FileController(
            IFileService fileService,
            ILogger<FileController> logger,
            IBuilderFactory builderFactory,
			ICensorFactory censorFactory,
            AuthContextBuilder contextBuilder,
            IAuthorizationContentResolver AuthorizationContentResolver,
            ClaimsExtractor claimsExtractor, 
            IFileAccessService accessService,
            IQueryFactory queryFactory)
        {
            _fileService = fileService;
            _logger = logger;
            _builderFactory = builderFactory;
            _censorFactory = censorFactory;
            _contextBuilder = contextBuilder;
            _authorizationContentResolver = AuthorizationContentResolver;
            _claimsExtractor = claimsExtractor;
            _accessService = accessService;
            _queryFactory = queryFactory;
        }

        [HttpPost("persist/temporary/many")]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        [RateLimit(RateLimitLevel.Restrictive)]
        public async Task<IActionResult> PersistBatchTemporarily()
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            IFormFileCollection files = Request.Form.Files;

            // Early validation for empty or null input
            if (files == null || files.Count == 0) return BadRequest("No files provided for upload.");

            List<Models.File.FilePersist> filesPersisted = await _fileService.SaveTemporarily([..files]);

            await _accessService.AttachUrlsAsync(filesPersisted);

            return Ok(filesPersisted);
        }

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

            await _accessService.AttachUrlsAsync(datas);

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

		[HttpGet("{id}")]
		[Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> GetFile(String id, [FromQuery] List<String> fields)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

            FileLookup lookup = new FileLookup
            {
                Offset = 0,
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

        [HttpPost("delete/{id}")]
        [Authorize]
        [RateLimit(RateLimitLevel.Restrictive)]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> Delete([FromRoute] String id)
        {
            if (String.IsNullOrEmpty(id) || !ModelState.IsValid) return BadRequest(ModelState);

            await _fileService.Delete(id);

            return Ok();
        }
	}
}
