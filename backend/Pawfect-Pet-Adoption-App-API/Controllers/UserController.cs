using Microsoft.AspNetCore.Mvc;
using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Models.User;
using Pawfect_Pet_Adoption_App_API.Services;

namespace Pawfect_Pet_Adoption_App_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception e)
            {
                return RequestHandlerTool.HandleInternalServerError(e, "GET");
            }
        }

        [HttpGet("id")]
        public async Task<IActionResult> GetUserById([FromQuery] string id)
        {

            try
            {
                if (await _userService.GetUserByIdAsync(id) is UserDto user && user != null)
                {
                    return Ok(user);
                }

                return NotFound();

            }
            catch (Exception e)
            {
                return RequestHandlerTool.HandleInternalServerError(e, "GET");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserPersist user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(user);
        }
    }
}
