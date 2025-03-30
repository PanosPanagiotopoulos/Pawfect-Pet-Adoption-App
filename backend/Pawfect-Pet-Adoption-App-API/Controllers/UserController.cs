using Microsoft.AspNetCore.Mvc;

using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.User;
using Pawfect_Pet_Adoption_App_API.Services.UserServices;

namespace Pawfect_Pet_Adoption_App_API.Controllers
{
	[ApiController]
	[Route("api/users")]
	public class UserController : ControllerBase
	{
		private readonly IUserService _userService;
		private readonly ILogger<UserController> _logger;

		public UserController(IUserService userService, ILogger<UserController> logger)
		{
			_userService = userService;
			_logger = logger;
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
				IEnumerable<UserDto>? models = await _userService.QueryUsersAsync(userLookup);

				if (models == null)
				{
					return NotFound();
				}

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
				UserDto? model = await _userService.Get(id, fields);

				if (model == null)
				{
					return NotFound();
				}

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
	}
}