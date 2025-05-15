using Microsoft.AspNetCore.Mvc;
using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Models.AdoptionApplication;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.User;
using Pawfect_Pet_Adoption_App_API.Query;
using Pawfect_Pet_Adoption_App_API.Services.UserServices;

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

        public UserController
			(
			IUserService userService, ILogger<UserController> logger,
			IQueryFactory queryFactory, IBuilderFactory builderFactory
			
			)
		{
			_userService = userService;
			_logger = logger;
            _queryFactory = queryFactory;
            _builderFactory = builderFactory;
        }

		/// <summary>
		/// Query χρήστες.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpPost("query")]
		[ProducesResponseType(200, Type = typeof(IEnumerable<UserDto>))]
		[ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
		[ProducesResponseType(404)]
		[ProducesResponseType(500, Type = typeof(String))]
		public async Task<IActionResult> QueryUsers([FromBody] UserLookup userLookup)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
                List<Data.Entities.User> datas = await userLookup
                                                        .EnrichLookup(_queryFactory)
                                                        .Authorise(Data.Entities.Types.Authorisation.AuthorizationFlags.OwnerOrPermissionOrAffiliation)
                                                        .CollectAsync();

                List<UserDto> models = await _builderFactory.Builder<UserBuilder>()
                                                    .Authorise(Data.Entities.Types.Authorisation.AuthorizationFlags.OwnerOrPermissionOrAffiliation)
                                                    .BuildDto(datas, userLookup.Fields.ToList());

                if (models == null) return NotFound();

                return Ok(models);
            }
			catch (Exception e)
			{
				_logger.LogError(e, "Error καθώς κάναμε query users");
				return RequestHandlerTool.HandleInternalServerError(e, "GET");
			}
		}

		/// <summary>
		/// Query χρήστες.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpGet("{id}")]
		[ProducesResponseType(200, Type = typeof(UserDto))]
		[ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
		[ProducesResponseType(404)]
		[ProducesResponseType(500, Type = typeof(String))]
		public async Task<IActionResult> GetUser(String id, [FromQuery] List<String> fields)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			try
			{
                UserLookup lookup = new UserLookup();

                // Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
                lookup.Offset = 1;
                // Γενική τιμή για τη λήψη των dtos
                lookup.PageSize = 1;
                lookup.Ids = [id];
                lookup.Fields = fields;

                UserDto model = (await _builderFactory.Builder<UserBuilder>().BuildDto(await lookup.EnrichLookup(_queryFactory).CollectAsync(), fields)).FirstOrDefault();

                if (model == null) return NotFound();


                return Ok(model);
            }
			catch (InvalidDataException e)
			{
				_logger.LogError(e, "Δεν βρέθηκε χρήστης");
				return NotFound();
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error καθώς κάναμε query user");
				return RequestHandlerTool.HandleInternalServerError(e, "GET");
			}
		}

		/// <summary>
		/// Delete a user by ID.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 404 NotFound, 500 String
		/// </summary>
		[HttpPost("delete")]
		[ProducesResponseType(200)]
		[ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
		[ProducesResponseType(404)]
		[ProducesResponseType(500, Type = typeof(String))]
		public async Task<IActionResult> Delete([FromBody] String id)
		{
			// TODO: Add authorization
			if (String.IsNullOrEmpty(id) || !ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				await _userService.Delete(id);
				return Ok();
			}
			catch (InvalidOperationException e)
			{
				_logger.LogError(e, "Αποτυχία διαγραφής χρήστη με ID {Id}", id);
				return NotFound();
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error ενώ κάναμε delete χρήστη με ID {Id}", id);
				return RequestHandlerTool.HandleInternalServerError(e, "POST");
			}
		}

		/// <summary>
		/// Delete multiple users by IDs.
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
			if (ids == null || !ids.Any() || !ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				await _userService.Delete(ids);
				return Ok();
			}
			catch (InvalidOperationException e)
			{
				_logger.LogError(e, "Αποτυχία διαγραφής χρηστών με IDs {Ids}", String.Join(", ", ids));
				return NotFound();
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error ενώ κάναμε delete πολλαπλών χρηστών με IDs {Ids}", String.Join(", ", ids));
				return RequestHandlerTool.HandleInternalServerError(e, "POST");
			}
		}
	}
}