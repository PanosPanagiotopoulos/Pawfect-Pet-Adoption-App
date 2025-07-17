using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Main_API.Builders;
using Main_API.Censors;
using Main_API.Data.Entities.Types.Authorization;
using Main_API.DevTools;
using Main_API.Exceptions;
using Main_API.Models.Lookups;
using Main_API.Models.User;
using Main_API.Query;
using Main_API.Services.AuthenticationServices;
using Main_API.Services.Convention;
using Main_API.Services.UserServices;
using Main_API.Transactions;
using System.Security.Claims;
using Pawfect_Pet_Adoption_App_API.Query;
using Main_API.Query.Queries;

namespace Main_API.Controllers
{
	[ApiController]
	[Route("api/users")]
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

        public UserController
			(
			    IUserService userService, ILogger<UserController> logger,
			    IQueryFactory queryFactory, IBuilderFactory builderFactory,
			    ICensorFactory censorFactory, AuthContextBuilder contextBuilder,
                IConventionService conventionService, IAuthorizationContentResolver AuthorizationContentResolver,
                ClaimsExtractor claimsExtractor

            )
		{
			_userService = userService;
			_logger = logger;
            _queryFactory = queryFactory;
            _builderFactory = builderFactory;
            _censorFactory = censorFactory;
            _contextBuilder = contextBuilder;
            _conventionService = conventionService;
            _authorizationContentResolver = AuthorizationContentResolver;
            _claimsExtractor = claimsExtractor;
        }

		/// <summary>
		/// Query χρήστες.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
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

		/// <summary>
		/// Query χρήστες.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpGet("{id}")]
		[Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> GetUser(String id, [FromQuery] List<String> fields)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

            UserLookup lookup = new UserLookup();

            // Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
            lookup.Offset = 1;
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

        /// <summary>
        /// Query χρήστες.
        /// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMe([FromQuery] List<String> fields)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            ClaimsPrincipal currentUser = _authorizationContentResolver.CurrentPrincipal();
            String userId = _claimsExtractor.CurrentUserId(currentUser);
            if (!_conventionService.IsValidId(userId)) throw new UnAuthenticatedException("User is not authenticated.");

            UserLookup lookup = new UserLookup();
            // Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
            lookup.Offset = 1;
            // Γενική τιμή για τη λήψη των dtos
            lookup.PageSize = 1;
            lookup.Ids = [userId];
            lookup.Fields = fields;

            AuthContext context = _contextBuilder.OwnedFrom(lookup, userId).Build();
            List<String> censoredFields = await _censorFactory.Censor<UserCensor>().Censor(BaseCensor.PrepareFieldsList([.. lookup.Fields]), context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying users");

            lookup.Fields = censoredFields;
            User model = (await _builderFactory.Builder<UserBuilder>().Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
                                .Build(await lookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation).CollectAsync(), censoredFields))
                                .FirstOrDefault();

            if (model == null) throw new NotFoundException("Shelter not found", JsonHelper.SerializeObjectFormatted(lookup), typeof(Data.Entities.Shelter));

            return Ok(model);
        }

        /// <summary>
        /// Delete a user by ID.
        /// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 404 NotFound, 500 String
        /// </summary>
        [HttpPost("delete")]
		[Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> Delete([FromBody] String id)
		{
			if (String.IsNullOrEmpty(id) || !ModelState.IsValid) return BadRequest(ModelState);

			await _userService.Delete(id);

			return Ok();
		}

		/// <summary>
		/// Delete multiple users by IDs.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 404 NotFound, 500 String
		/// </summary>
		[HttpPost("delete/many")]
		[Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> DeleteMany([FromBody] List<String> ids)
		{
			// TODO: Add authorization
			if (ids == null || ids.Count == 0 || !ModelState.IsValid) return BadRequest(ModelState);

			await _userService.Delete(ids);

			return Ok();
		}
	}
}