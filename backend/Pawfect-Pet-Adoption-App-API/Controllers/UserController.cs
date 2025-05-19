using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Censors;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Exceptions;
using Pawfect_Pet_Adoption_App_API.Models.AdoptionApplication;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.User;
using Pawfect_Pet_Adoption_App_API.Query;
using Pawfect_Pet_Adoption_App_API.Services.UserServices;
using Pawfect_Pet_Adoption_App_API.Transactions;
using System.Reflection;

namespace Pawfect_Pet_Adoption_App_API.Controllers
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

        public UserController
			(
			IUserService userService, ILogger<UserController> logger,
			IQueryFactory queryFactory, IBuilderFactory builderFactory,
			ICensorFactory censorFactory, AuthContextBuilder contextBuilder

            )
		{
			_userService = userService;
			_logger = logger;
            _queryFactory = queryFactory;
            _builderFactory = builderFactory;
            _censorFactory = censorFactory;
            _contextBuilder = contextBuilder;
        }

		/// <summary>
		/// Query χρήστες.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpPost("query")]
		[Authorize]
		public async Task<IActionResult> QueryUsers([FromBody] UserLookup userLookup)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

            AuthContext context = _contextBuilder.OwnedFrom(userLookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<UserCensor>().Censor([.. userLookup.Fields], context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying users");

            userLookup.Fields = censoredFields;
            List<Data.Entities.User> datas = await userLookup
                                                    .EnrichLookup(_queryFactory)
                                                    .Authorise(AuthorizationFlags.OwnerOrPermission)
                                                    .CollectAsync();

            List<User> models = await _builderFactory.Builder<UserBuilder>()
                                                .Authorise(AuthorizationFlags.OwnerOrPermission)
                                                .Build(datas, [.. userLookup.Fields]);

            if (models == null) throw new NotFoundException("Users not found", JsonHelper.SerializeObjectFormatted(userLookup), typeof(Data.Entities.User));

            return Ok(models);
		}

		/// <summary>
		/// Query χρήστες.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpGet("{id}")]
		[Authorize]
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
            List<String> censoredFields = await _censorFactory.Censor<UserCensor>().Censor([.. lookup.Fields], context);
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