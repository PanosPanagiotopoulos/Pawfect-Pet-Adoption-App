using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Main_API.Builders;
using Main_API.Censors;
using Main_API.Data.Entities.Types.Authorization;
using Main_API.DevTools;
using Main_API.Exceptions;
using Main_API.Models.Lookups;
using Main_API.Models.Shelter;
using Main_API.Query;
using Main_API.Services.ShelterServices;
using Main_API.Transactions;
using Pawfect_Pet_Adoption_App_API.Query;
using Main_API.Query.Queries;
using Main_API.Services.AuthenticationServices;
using Main_API.Services.Convention;
using Microsoft.Extensions.Options;
using Main_API.Data.Entities.Types.Cache;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;

namespace Main_API.Controllers
{
	[ApiController]
	[Route("api/shelters")]
	public class ShelterController : ControllerBase
	{
		private readonly IShelterService _shelterService;
		private readonly ILogger<ShelterController> _logger;
        private readonly IQueryFactory _queryFactory;
        private readonly IBuilderFactory _builderFactory;
        private readonly ICensorFactory _censorFactory;
        private readonly AuthContextBuilder _contextBuilder;
        private readonly IAuthorizationContentResolver _authorizationContentResolver;
        private readonly ClaimsExtractor _claimsExtractor;
        private readonly IMemoryCache _memoryCache;
        private readonly CacheConfig _cacheConfig;
        private readonly IConventionService _conventionService;

        public ShelterController
		(
			IShelterService shelterService,
            ILogger<ShelterController> logger,
			IQueryFactory queryFactory, 
            IBuilderFactory builderFactory,
            ICensorFactory censorFactory, 
            AuthContextBuilder contextBuilder,
            IAuthorizationContentResolver authorizationContentResolver,
            ClaimsExtractor claimsExtractor,
            IMemoryCache memoryCache,
            IOptions<CacheConfig> cacheOptions,
            IConventionService conventionService
        )
		{
			_shelterService = shelterService;
			_logger = logger;
            _queryFactory = queryFactory;
            _builderFactory = builderFactory;
            _censorFactory = censorFactory;
            _contextBuilder = contextBuilder;
            _authorizationContentResolver = authorizationContentResolver;
            _claimsExtractor = claimsExtractor;
            _memoryCache = memoryCache;
            _cacheConfig = cacheOptions.Value;
            _conventionService = conventionService;
        }

		[HttpPost("query")]
		[Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> QueryShelters([FromBody] ShelterLookup shelterLookup)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

            AuthContext context = _contextBuilder.OwnedFrom(shelterLookup).AffiliatedWith(shelterLookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<ShelterCensor>().Censor([.. shelterLookup.Fields], context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying shelters");

            shelterLookup.Fields = censoredFields;

            ClaimsPrincipal claimsPrincipal = _authorizationContentResolver.CurrentPrincipal();
            String userId = _claimsExtractor.CurrentUserId(claimsPrincipal);
            if (!_conventionService.IsValidId(userId)) throw new ForbiddenException();

            String cacheKey = $"{userId}_{shelterLookup.GetHashCode()}";
            QueryResult<Shelter> queryResult = null;
            if (_memoryCache.TryGetValue(cacheKey, out String queryResultValue))
            {
                queryResult = JsonHelper.DeserializeObjectFormattedSafe<QueryResult<Shelter>>(queryResultValue);
                if (shelterLookup?.ExcludedIds?.Any() == true)
                    queryResult.Items = queryResult.Items.Where(shelter => !shelterLookup.ExcludedIds.Contains(shelter.Id)).ToList();
            }

            ShelterQuery q = shelterLookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation);

            List<Data.Entities.Shelter> datas = await q.CollectAsync();

            List<Shelter> models = await _builderFactory.Builder<ShelterBuilder>()
                                                .Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
                                                .Build(datas, [.. shelterLookup.Fields]);

            if (models == null) throw new NotFoundException("Shelters not found", JsonHelper.SerializeObjectFormatted(shelterLookup), typeof(Data.Entities.Shelter));

            queryResult = new QueryResult<Shelter>()
            {
                Items = models,
                Count = await q.CountAsync()
            };

            _memoryCache.Set(cacheKey, JsonHelper.SerializeObjectFormatted(queryResult), TimeSpan.FromMinutes(_cacheConfig.QueryCacheTime));

            return Ok(queryResult);
        }

		[HttpGet("{id}")]
		[Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> GetShelter(String id, [FromQuery] List<String> fields)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

            ShelterLookup lookup = new ShelterLookup();

            // Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
            lookup.Offset = 1;
            // Γενική τιμή για τη λήψη των dtos
            lookup.PageSize = 1;
            lookup.Ids = [id];
            lookup.Fields = fields;

            AuthContext context = _contextBuilder.OwnedFrom(lookup).AffiliatedWith(lookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<ShelterCensor>().Censor(BaseCensor.PrepareFieldsList([.. lookup.Fields]), context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying shelters");

            lookup.Fields = censoredFields;
            Shelter model = (await _builderFactory.Builder<ShelterBuilder>().Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
									.Build(await lookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation).CollectAsync(), censoredFields))
									.FirstOrDefault();

            if (model == null) throw new NotFoundException("Shelter not found", JsonHelper.SerializeObjectFormatted(lookup), typeof(Data.Entities.Shelter));


            return Ok(model);
		}

        [HttpPost("persist")]
		[Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> Persist([FromBody] ShelterPersist model, [FromQuery] List<String> fields)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			fields = BaseCensor.PrepareFieldsList(fields);

            Shelter shelter = await _shelterService.Persist(model, fields);

			return Ok(shelter);
			
		}

        [HttpPost("delete/{id}")]
        [Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> Delete([FromRoute] String id)
        {
            if (String.IsNullOrEmpty(id) || !ModelState.IsValid) return BadRequest(ModelState);

            await _shelterService.Delete(id);

            return Ok();
        }
    }
}
