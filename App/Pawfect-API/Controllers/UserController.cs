using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawfect_API.Builders;
using Pawfect_API.Censors;
using Pawfect_API.Data.Entities.Types.Authorization;
using Pawfect_API.DevTools;
using Pawfect_API.Exceptions;
using Pawfect_API.Models.Lookups;
using Pawfect_API.Models.User;
using Pawfect_API.Query;
using Pawfect_API.Services.AuthenticationServices;
using Pawfect_API.Services.Convention;
using Pawfect_API.Services.UserServices;
using Pawfect_API.Transactions;
using System.Security.Claims;
using Pawfect_Pet_Adoption_App_API.Query;
using Pawfect_API.Query.Queries;
using Pawfect_Pet_Adoption_App_API.Models.User;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Pawfect_API.Data.Entities.Types.Cache;
using Pawfect_Pet_Adoption_App_API.Attributes;
using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;

namespace Pawfect_API.Controllers
{
	[ApiController]
	[Route("api/users")]
    [RateLimit(RateLimitLevel.Moderate)]
    public class UserController : ControllerBase
	{
		private readonly IUserService _userService;
		private readonly ILogger<UserController> _logger;
        private readonly IQueryFactory _queryFactory;
        private readonly IBuilderFactory _builderFactory;
        private readonly ICensorFactory _censorFactory;
        private readonly AuthContextBuilder _contextBuilder;
        private readonly IConventionService _conventionService;
        private readonly IAuthorizationContentResolver _authorizationContentResolver;
        private readonly ClaimsExtractor _claimsExtractor;
        private readonly IMemoryCache _memoryCache;
        private readonly CacheConfig _cacheConfig;

        public UserController
		(
			IUserService userService, ILogger<UserController> logger,
			IQueryFactory queryFactory, IBuilderFactory builderFactory,
			ICensorFactory censorFactory, AuthContextBuilder contextBuilder,
            IConventionService conventionService, IAuthorizationContentResolver authorizationContentResolver,
            ClaimsExtractor claimsExtractor, IMemoryCache memoryCache, 
            IOptions<CacheConfig> cacheOptions

        )
		{
			_userService = userService;
			_logger = logger;
            _queryFactory = queryFactory;
            _builderFactory = builderFactory;
            _censorFactory = censorFactory;
            _contextBuilder = contextBuilder;
            _conventionService = conventionService;
            _authorizationContentResolver = authorizationContentResolver;
            _claimsExtractor = claimsExtractor;
            _memoryCache = memoryCache;
            _cacheConfig = cacheOptions.Value;
        }

		[HttpPost("query")]
		[Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> QueryUsers([FromBody] UserLookup userLookup)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

            AuthContext context = _contextBuilder.OwnedFrom(userLookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<UserCensor>().Censor([.. userLookup.Fields], context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying users");

            userLookup.Fields = censoredFields;
            UserQuery q = userLookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation);

            List<Data.Entities.User> datas = await q.CollectAsync();

            List<User> models = await _builderFactory.Builder<UserBuilder>()
                                                .Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
                                                .Build(datas, [.. userLookup.Fields]);

            if (models == null) throw new NotFoundException("Users not found", JsonHelper.SerializeObjectFormatted(userLookup), typeof(Data.Entities.User));

            return Ok(new QueryResult<User>()
            {
                Items = models,
                Count = await q.CountAsync()
            });
		}

		[HttpGet("{id}")]
		[Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> GetUser(String id, [FromQuery] List<String> fields)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

            UserLookup lookup = new UserLookup();

            // Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
            lookup.Offset = 0;
            // Γενική τιμή για τη λήψη των dtos
            lookup.PageSize = 1;
            lookup.Ids = [id];
            lookup.Fields = fields;

            AuthContext context = _contextBuilder.OwnedFrom(lookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<UserCensor>().Censor(BaseCensor.PrepareFieldsList([.. lookup.Fields]), context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying users");

            lookup.Fields = censoredFields;
            User model = (await _builderFactory.Builder<UserBuilder>().Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
								.Build(await lookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation).CollectAsync(), censoredFields))
                                .FirstOrDefault();

            if (model == null) throw new NotFoundException("Shelter not found", JsonHelper.SerializeObjectFormatted(lookup), typeof(Data.Entities.Shelter));

            return Ok(model);
		}

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMe([FromQuery] List<String> fields)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            ClaimsPrincipal currentUser = _authorizationContentResolver.CurrentPrincipal();
            String userId = _claimsExtractor.CurrentUserId(currentUser);
            if (!_conventionService.IsValidId(userId)) throw new UnAuthenticatedException("User is not authenticated.");

            User model = null;

            String cacheKey = $"User_Profile_{userId}_[{String.Join('|', fields)}]";
            if (_memoryCache.TryGetValue(cacheKey, out String profileData))
            {
                model = JsonHelper.DeserializeObjectFormattedSafe<User>(profileData);
                if (model != null)
                    return Ok(model);
            }

            UserLookup lookup = new UserLookup();
            // Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
            lookup.Offset = 0;
            // Γενική τιμή για τη λήψη των dtos
            lookup.PageSize = 1;
            lookup.Ids = [userId];
            lookup.Fields = fields;

            AuthContext context = _contextBuilder.OwnedFrom(lookup, userId).Build();
            List<String> censoredFields = await _censorFactory.Censor<UserCensor>().Censor(BaseCensor.PrepareFieldsList([.. lookup.Fields]), context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying users");

            lookup.Fields = censoredFields;
            model = (await _builderFactory.Builder<UserBuilder>().Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
                                .Build(await lookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation).CollectAsync(), censoredFields))
                                .FirstOrDefault();

            if (model == null) throw new NotFoundException("User not found", JsonHelper.SerializeObjectFormatted(lookup), typeof(Data.Entities.User));

            _memoryCache.Set(cacheKey, JsonHelper.SerializeObjectFormattedSafe(model), TimeSpan.FromMinutes(_cacheConfig.QueryCacheTime));

            return Ok(model);
        }

        [HttpPost("update")]
        [Authorize]
        [RateLimit(RateLimitLevel.Restrictive)]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> Persist([FromBody] UserUpdate model, [FromQuery] List<String> fields)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            fields = BaseCensor.PrepareFieldsList(fields);

            User animal = await _userService.Update(model, fields);

            return Ok(animal);
        }

        [HttpPost("delete")]
		[Authorize]
        [RateLimit(RateLimitLevel.Restrictive)]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> Delete([FromBody] String id)
		{
			if (String.IsNullOrEmpty(id) || !ModelState.IsValid) return BadRequest(ModelState);

			await _userService.Delete(id);

			return Ok();
		}

		[HttpPost("delete/many")]
		[Authorize]
        [RateLimit(RateLimitLevel.Restrictive)]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> DeleteMany([FromBody] List<String> ids)
		{
			if (ids == null || ids.Count == 0 || !ModelState.IsValid) return BadRequest(ModelState);

			await _userService.Delete(ids);

			return Ok();
		}
	}
}