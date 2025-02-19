using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Models;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.User;
using Pawfect_Pet_Adoption_App_API.Services.UserServices;

namespace Pawfect_Pet_Adoption_App_API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class UserController : ControllerBase
	{
		private readonly IUserService _userService;
		private readonly ILogger<UserController> _logger;
		private readonly IMemoryCache _memoryCache;

		public UserController(IUserService userService, ILogger<UserController> logger, IMemoryCache memoryCache)
		{
			_userService = userService;
			_logger = logger;
			_memoryCache = memoryCache;
		}

		/// <summary>
		/// Query χρήστες.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpGet]
		[ProducesResponseType(200, Type = typeof(IEnumerable<UserDto>))]
		[ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
		[ProducesResponseType(404)]
		[ProducesResponseType(500, Type = typeof(String))]
		public async Task<IActionResult> QueryUsers([FromQuery] UserLookup userLookup)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				IEnumerable<UserDto>? queriedUsers = await _userService.QueryUsersAsync(userLookup);

				if (queriedUsers == null)
				{
					return NotFound();
				}

				return Ok(queriedUsers);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error καθώς κάναμε query users");
				return RequestHandlerTool.HandleInternalServerError(e, "GET");
			}
		}

		/// <summary>
		/// Εγγραφή μη επιβεβαιωμένου χρήστη.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpPost("register/unverified")]
		[ProducesResponseType(200, Type = typeof(String))]
		[ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
		[ProducesResponseType(500, Type = typeof(String))]
		public async Task<IActionResult> RegisterUserUnverified([FromBody] RegisterPersist toRegisterUser)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				return Ok(await _userService.RegisterUserUnverifiedAsync(toRegisterUser));
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error while registering unverified user");
				return RequestHandlerTool.HandleInternalServerError(e, "POST");
			}
		}

		/// <summary>
		/// Αποστολή OTP στον αριθμό τηλεφώνου του χρήστη.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpPost("send/otp")]
		[ProducesResponseType(200)]
		[ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
		[ProducesResponseType(500, Type = typeof(String))]
		public async Task<IActionResult> SendOtp([FromBody] OtpPayload otpPayload)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				await _userService.GenerateNewOtpAsync(otpPayload.Phone);
				return Ok();
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error στην αποστολή OTP σε χρήστη");
				return RequestHandlerTool.HandleInternalServerError(e, "POST");
			}
		}

		/// <summary>
		/// Επιβεβαίωση OTP χρήστη.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpPost("verify-otp")]
		[ProducesResponseType(200)]
		[ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
		[ProducesResponseType(500, Type = typeof(String))]
		public async Task<IActionResult> VerifyUserOtp([FromBody] OtpPayload otpPayload)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				if (!_userService.VerifyOtp(otpPayload.Phone, otpPayload.Otp))
				{
					// LOGS //
					_logger.LogError("Αποτυχία επιβεβαίωσης OTP");
					ModelState.AddModelError("error", $"Αποτυχία επιβεβαίωσης OTP: {otpPayload.Otp}");
					return BadRequest(ModelState);
				}

				User? verifyPhoneUser = null;
				if ((verifyPhoneUser = (await _userService.RetrieveUserAsync(otpPayload.Id, otpPayload.Email))) == null)
				{
					// LOGS //
					_logger.LogError("Failed to verify phonenumber. The user with this email or id was not found");
					ModelState.AddModelError("error", "Failed to verify phonenumber. The user with this email or id was not found");
					return BadRequest(ModelState);
				}

				verifyPhoneUser.HasPhoneVerified = true;
				if ((await _userService.PersistUserAsync(verifyPhoneUser, false) is String userId && String.IsNullOrEmpty(userId)))
				{
					// LOGS //
					_logger.LogError("Failed to verify phonenumber. The user with this email or id failed to update");

					return RequestHandlerTool.HandleInternalServerError(new Exception("Failed to verify phonenumber. The user with this email or id failed to update"), "POST");
				}

				return Ok();
			}
			catch (Exception e)
			{
				// LOGS //
				_logger.LogError(e, "Error while verifying user OTP");
				return RequestHandlerTool.HandleInternalServerError(e, "POST");
			}
		}

		/// <summary>
		/// Αποστολή επιβεβαίωσης email.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpPost("send/email-verification")]
		[ProducesResponseType(200)]
		[ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
		[ProducesResponseType(500, Type = typeof(String))]
		public async Task<IActionResult> SendEmailVerification([FromBody] EmailPayload emailPayload)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				await _userService.SendVerficationEmailAsync(emailPayload.Email);
				return Ok();
			}
			catch (Exception e)
			{
				// LOGS //
				_logger.LogError(e, "Αποτυχία αποστολής email επιβεβαίωσης");
				return RequestHandlerTool.HandleInternalServerError(e, "POST");
			}
		}

		/// <summary>
		/// Επιβεβαίωση email χρήστη.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpPost("verify-email")]
		[ProducesResponseType(200)]
		[ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
		[ProducesResponseType(500, Type = typeof(String))]
		public async Task<IActionResult> VerifyEmail([FromBody] EmailPayload emailPayload)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				if (!_userService.VerifyEmail(emailPayload.Email, emailPayload.Token))
				{
					// LOGS //
					_logger.LogError("Failed to verify email");
					ModelState.AddModelError("error", "Failed to verify email");
					return BadRequest(ModelState);
				}

				User? verifyEmailUser = null;
				if ((verifyEmailUser = (await _userService.RetrieveUserAsync(emailPayload.Id, emailPayload.Email))) == null)
				{
					// LOGS //
					_logger.LogError("Failed to verify email. The user with this email or id was not found");
					ModelState.AddModelError("error", "Failed to verify email. The user with this email or id was not found");
					return BadRequest(ModelState);
				}

				verifyEmailUser.HasEmailVerified = true;
				if ((await _userService.PersistUserAsync(verifyEmailUser, false) is String userId && String.IsNullOrEmpty(userId)))
				{
					// LOGS //
					_logger.LogError("Failed to verify email. The user with this email or id failed to update");

					return RequestHandlerTool.HandleInternalServerError(new Exception("Failed to verify email. The user with this email or id failed to update"), "POST");
				}

				return Ok();
			}

			catch (Exception e)
			{
				// LOGS //
				_logger.LogError(e, "Error while verifying email");
				return RequestHandlerTool.HandleInternalServerError(e, "POST");
			}
		}

		/// <summary>
		/// Εγγραφή επιβεβαιωμένου χρήστη.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpPost("register/verified")]
		[ProducesResponseType(200)]
		[ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
		[ProducesResponseType(500, Type = typeof(String))]
		public async Task<IActionResult> RegisterUserVerified([FromBody] EmailPayload toVerifyUserData)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				if (!(await _userService.VerifyUserAsync(toVerifyUserData.Id, toVerifyUserData.Email)))
				{
					// LOGS //
					_logger.LogError("Error προσπαθόντας να κάνουμε verify τον χρήστη");
					ModelState.AddModelError("error", "Έλειψη απαραίτητων κριτηρίων για πλήρη επιβεβαίωση χρήστη");
					return BadRequest(ModelState);
				}

				return Ok();
			}
			catch (Exception e)
			{
				// LOGS //
				_logger.LogError(e, "Error προσπαθόντας να κάνουμε verify τον χρήστη");
				return RequestHandlerTool.HandleInternalServerError(e, "POST");
			}
		}

		/// <summary>
		/// Αποστολή email επαναφοράς κωδικού πρόσβασης.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpPost("send/reset-password")]
		[ProducesResponseType(200)]
		[ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
		[ProducesResponseType(500, Type = typeof(String))]
		public async Task<IActionResult> SendResetPasswordEmail([FromBody] ResetPasswordPayload emailPayload)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				// Αποστολή reset-password email για έναρξη επαναφοράς κωδικού
				await _userService.SendResetPasswordEmailAsync(emailPayload.Email);
				return Ok();
			}
			catch (Exception e)
			{
				// LOGS //
				_logger.LogError(e, "Αποτυχία αποστολής reset password email");
				return RequestHandlerTool.HandleInternalServerError(e, "POST");
			}
		}

		/// <summary>
		/// Επαναφορά κωδικού πρόσβασης χρήστη.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpPost("reset-password")]
		[ProducesResponseType(302)]
		[ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
		[ProducesResponseType(500, Type = typeof(String))]
		public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordPayload resetPasswordPayload)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				// Reset the password
				if (!await _userService.ResetPasswordAsync(resetPasswordPayload.Email, resetPasswordPayload.NewPassword, resetPasswordPayload.Token))
				{
					// LOGS //
					_logger.LogError("Αποτυχία επαναφοράς κωδικού");
					ModelState.AddModelError("error", "Αποτυχία επαναφοράς κωδικού");
					return BadRequest(ModelState);
				}

				// Redirect στο login page που βρίσκεται στο : /auth/login αν είναι επιτυχής η επαναφορά
				return Redirect("/auth/login");
			}
			catch (Exception e)
			{
				// LOGS //
				_logger.LogError(e, "Αποτυχία επαναφοράς κωδικού");
				return RequestHandlerTool.HandleInternalServerError(e, "POST");
			}
		}
	}
}