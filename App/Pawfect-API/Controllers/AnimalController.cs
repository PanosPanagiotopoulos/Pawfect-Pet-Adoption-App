using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawfect_API.Builders;
using Pawfect_API.Censors;
using Pawfect_API.Data.Entities.Types.Authorization;
using Pawfect_API.DevTools;
using Pawfect_API.Exceptions;
using Pawfect_API.Models.Animal;
using Pawfect_API.Models.Lookups;
using Pawfect_API.Query;
using Pawfect_API.Services.AnimalServices;
using Pawfect_API.Transactions;
using Pawfect_API.Query;
using Pawfect_API.Query.Queries;
using Microsoft.Extensions.Options;
using Pawfect_API.Data.Entities.Types.Authorisation;
using Microsoft.Extensions.Caching.Memory;
using Pawfect_API.Services.AuthenticationServices;
using System.Security.Claims;
using Pawfect_API.Data.Entities.Types.Cache;
using Pawfect_API.Services.Convention;
using Pawfect_API.Services.FileServices;
using Pawfect_API.Attributes;
using Pawfect_API.Data.Entities.EnumTypes;

namespace Pawfect_API.Controllers
{
	[ApiController]
	[Route("api/animals")]
    [RateLimit(RateLimitLevel.Moderate)]
    public class AnimalController : ControllerBase
	{
        private readonly IAnimalService _animalService;
        private readonly ILogger<AnimalController> _logger;
        private readonly IBuilderFactory _builderFactory;
        private readonly ICensorFactory _censorFactory;
        private readonly AuthContextBuilder _contextBuilder;
        private readonly Lazy<IFileDataExtractor> _excelExtractor;
        private readonly IMemoryCache _memoryCache;
        private readonly IQueryFactory _queryFactory;
        private readonly CacheConfig _cacheConfig;
        private readonly IAuthorizationContentResolver _authorizationContentResolver;
        private readonly ClaimsExtractor _claimsExtractor;
        private readonly IConventionService _conventionService;
        private readonly AnimalsConfig _animalsConfig;

        public AnimalController
        (
            IAnimalService animalService,
            ILogger<AnimalController> logger,
            IBuilderFactory builderFactory,
			ICensorFactory censorFactory,
            AuthContextBuilder contextBuilder,
            Lazy<IFileDataExtractor> excelExtractor,
            IMemoryCache memoryCache,
            IQueryFactory queryFactory,
            IOptions<CacheConfig> cacheOptions, 
            IAuthorizationContentResolver authorizationContentResolver,
            ClaimsExtractor claimsExtractor,
            IConventionService conventionService,
            IOptions<AnimalsConfig> options
        )
        {
            _animalService = animalService;
            _logger = logger;
            _builderFactory = builderFactory;
            _censorFactory = censorFactory;
            _contextBuilder = contextBuilder;
            _excelExtractor = excelExtractor;
            _memoryCache = memoryCache;
            _queryFactory = queryFactory;
            _cacheConfig = cacheOptions.Value;
            _authorizationContentResolver = authorizationContentResolver;
            _claimsExtractor = claimsExtractor;
            _conventionService = conventionService;
            _animalsConfig = options.Value;
        }

        [HttpPost("query")]
        [Authorize]
        public async Task<IActionResult> QueryAnimals([FromBody] AnimalLookup animalLookup)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

            ClaimsPrincipal claimsPrincipal = _authorizationContentResolver.CurrentPrincipal();
            String userId = _claimsExtractor.CurrentUserId(claimsPrincipal);
            if (!_conventionService.IsValidId(userId)) throw new ForbiddenException();            

            String cacheKey = $"{userId}_{animalLookup.GetHashCode()}";
            QueryResult<Animal> queryResult = null;
            if (_memoryCache.TryGetValue(cacheKey, out String queryResultValue))
            {
                queryResult = JsonHelper.DeserializeObjectFormattedSafe<QueryResult<Animal>>(queryResultValue);
                if (queryResult != null)
                {
                    if (animalLookup?.ExcludedIds?.Any() == true)
                        queryResult.Items = queryResult.Items.Where(animal => !animalLookup.ExcludedIds.Contains(animal.Id)).ToList();

                    return Ok(queryResult);
                }
            }

            AuthContext context = _contextBuilder.OwnedFrom(animalLookup).AffiliatedWith(animalLookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<AnimalCensor>().Censor([.. animalLookup.Fields], context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying animals");

            animalLookup.Fields = censoredFields;
            AnimalQuery q = animalLookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation);
            
            List<Data.Entities.Animal> datas = await q.CollectAsync();

            List<Animal> models = await _builderFactory.Builder<AnimalBuilder>()
                .Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
                .Build(datas, [..animalLookup.Fields]);

            if (models == null) throw new NotFoundException("Animals not found", JsonHelper.SerializeObjectFormatted(animalLookup), typeof(Data.Entities.Animal));

            queryResult = new QueryResult<Animal>()
            {
                Items = models,
                Count = await q.CountAsync()
            };

            _memoryCache.Set(cacheKey, JsonHelper.SerializeObjectFormatted(queryResult), TimeSpan.FromMinutes(_cacheConfig.QueryCacheTime));

            return Ok(queryResult);
        }

        [HttpPost("query/free-view")]
        public async Task<IActionResult> QueryAnimalsFreeView([FromBody] AnimalLookup animalLookup)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            animalLookup.Fields = _animalsConfig.FreeFields;
            animalLookup.UseVectorSearch = true;
            animalLookup.UseSemanticSearch = true;

            String cacheKey = $"{animalLookup.GetHashCode()}";
            QueryResult<Animal> queryResult = null;
            if (_memoryCache.TryGetValue(cacheKey, out String queryResultValue))
            {
                queryResult = JsonHelper.DeserializeObjectFormattedSafe<QueryResult<Animal>>(queryResultValue);
                if (queryResult != null)
                {
                    if (animalLookup?.ExcludedIds?.Any() == true)
                        queryResult.Items = queryResult.Items.Where(animal => !animalLookup.ExcludedIds.Contains(animal.Id)).ToList();
                    
                    return Ok(queryResult);
                }
            }

            AnimalQuery q = animalLookup.EnrichLookup(_queryFactory);

            List<Data.Entities.Animal> datas = await q.CollectAsync();

            List<Animal> models = await _builderFactory.Builder<AnimalBuilder>()
                .Build(datas, [.. animalLookup.Fields]);

            if (models == null) throw new NotFoundException("Animals not found", JsonHelper.SerializeObjectFormatted(animalLookup), typeof(Data.Entities.Animal));

            queryResult = new QueryResult<Animal>()
            {
                Items = models,
                Count = await q.CountAsync()
            };

            _memoryCache.Set(cacheKey, JsonHelper.SerializeObjectFormatted(queryResult), TimeSpan.FromMinutes(_cacheConfig.QueryCacheTime));

            return Ok(queryResult);
        }

        [HttpGet("{id}")]
		[Authorize]
        public async Task<IActionResult> GetAnimal(String id, [FromQuery] List<String> fields)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

            AnimalLookup lookup = new AnimalLookup
            {
                Offset = 0,
                PageSize = 1,
                Ids = new List<String> { id },
                Fields = fields
            };

            AuthContext context = _contextBuilder.OwnedFrom(lookup).AffiliatedWith(lookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<AnimalCensor>().Censor(BaseCensor.PrepareFieldsList([.. lookup.Fields]), context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying animals");

            lookup.Fields = censoredFields;
            Animal model = (await _builderFactory.Builder<AnimalBuilder>().Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
                .Build(await lookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation).CollectAsync(), censoredFields))
            .FirstOrDefault();

            if (model == null) throw new NotFoundException("Animal not found", JsonHelper.SerializeObjectFormatted(lookup), typeof(Data.Entities.Animal));

            return Ok(model);
		}

		[HttpPost("persist")]
		[Authorize]
        [RateLimit(RateLimitLevel.Restrictive)]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> Persist([FromBody] AnimalPersist model, [FromQuery] List<String> fields)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

            fields = BaseCensor.PrepareFieldsList(fields);

            Animal animal = await _animalService.Persist(model, fields);

			return Ok(animal);
		}

        [HttpPost("persist/batch")]
        [Authorize]
        [RateLimit(RateLimitLevel.Restrictive)]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> PersistBatch([FromBody] List<AnimalPersist> model, [FromQuery] List<String> fields)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            fields = BaseCensor.PrepareFieldsList(fields);

            List<Animal> persisted = await _animalService.PersistBatch(model, fields);

            return Ok(persisted);
        }

        [HttpGet("import-template/excel")]
        [Authorize]
        [RateLimit(RateLimitLevel.Restrictive)]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> GetAnimalTempalteExcel()
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            Byte[] template = await _excelExtractor.Value.GenerateAnimalImportTemplate();

            FileContentResult result = new FileContentResult(template, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = "import_template.xlsx"
            };

            return result;
        }

        [HttpPost("from-template/excel")]
        [Authorize]
        [RateLimit(RateLimitLevel.Restrictive)]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> ImportFromExcelTemplate()
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            IFormFileCollection files = Request.Form.Files;
            if (files == null || files.Count != 1) return BadRequest("Invalid amount of files provided");

            IFormFile excelFile = files.FirstOrDefault();
            if (!Path.GetExtension(excelFile.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase) &&
                !Path.GetExtension(excelFile.FileName).Equals(".xls", StringComparison.OrdinalIgnoreCase)) 
                return BadRequest("Not excel file provided");

            List<AnimalPersist> exctractedModels = await this._excelExtractor.Value.ExtractAnimalModelData(excelFile);

            return Ok(exctractedModels);
        }

        [HttpPost("delete/{id}")]
		[Authorize]
        [RateLimit(RateLimitLevel.Restrictive)]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> Delete([FromRoute] String id)
		{
			if (String.IsNullOrEmpty(id) || !ModelState.IsValid) return BadRequest(ModelState);

			await _animalService.Delete(id);

			return Ok();
		}
	}
}
