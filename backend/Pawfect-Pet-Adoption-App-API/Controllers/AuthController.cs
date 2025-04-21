namespace Pawfect_Pet_Adoption_App_API.Controllers
{
	using AutoMapper;

	using Microsoft.AspNetCore.Authorization;
	using Microsoft.AspNetCore.Mvc;

	using Pawfect_Pet_Adoption_App_API.Data.Entities;
	using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
	using Pawfect_Pet_Adoption_App_API.DevTools;
	using Pawfect_Pet_Adoption_App_API.Models;
	using Pawfect_Pet_Adoption_App_API.Models.Authorization;
	using Pawfect_Pet_Adoption_App_API.Models.User;
	using Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices;
	using Pawfect_Pet_Adoption_App_API.Services.UserServices;

	using System.IdentityModel.Tokens.Jwt;

	[ApiController]
	[Route("auth")]
	public class AuthController : ControllerBase
	{
		private readonly IUserService _userService;
		private readonly ILogger<AuthController> _logger;
		private readonly JwtService _jwtService;
		private readonly IAuthService _authService;
		private readonly IMapper _mapper;

		public AuthController(
			IUserService userService, ILogger<AuthController> logger
			, JwtService jwtService, IAuthService authService
			, IMapper mapper
			)
		{
			_userService = userService;
			_logger = logger;
			_jwtService = jwtService;
			_authService = authService;
			_mapper = mapper;
		}

		/// <summary>
		/// Συνάρτηση για την εκτέλεση σύνδεσης χρήστη
		/// </summary>
		[HttpPost("login")]
		[ProducesResponseType(200)]
		[ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
		[ProducesResponseType(401, Type = typeof(String))]
		[ProducesResponseType(500, Type = typeof(String))]
		public async Task<IActionResult> Login([FromBody] AuthPayload payload)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				String? loginEmail = payload.Email;
				String? loginCredential = payload.Password;

				if (payload.LoginProvider == AuthProvider.Google)
				{
					(loginEmail, loginCredential) = await _userService.RetrieveGoogleCredentials(payload.ProviderAccessCode);
				}

				User? user = await _userService.RetrieveUserAsync(null, loginEmail);

				if (user == null)
				{
					return Unauthorized("Λάθος email χρήστη");
				}

				String? toCheckCredential = _userService.ExtractUserCredential(user);

				if (!Security.ValidatedHashedValues(loginCredential, toCheckCredential))
				{
					return Unauthorized("Λάθος credentials χρήστη");
				}

				if (!user.HasPhoneVerified)
				{
					return Unauthorized("Ο χρήστης δεν έχει επιβεβαιώσει τα στοιχεία του αριθμού τηλεφώνου του.");
				}

				String? token = _jwtService.GenerateJwtToken(user.Id, user.Email, user.Role.ToString(), user.HasEmailVerified.ToString(), user.IsVerified.ToString());
				if (token == null)
				{
					// LOGS //
					_logger.LogError("Αποτυχία παραγωγής JWT Token στο Login");
					return RequestHandlerTool.HandleInternalServerError(new InvalidOperationException("Αποτυχία παραγωγής JWT Token"), "POST");
				}

				return Ok(new LoggedAccount() { Token = token, Email = user.Email, Phone = user.Phone, Role = user.Role, LoggedAt = DateTime.UtcNow, IsEmailVerified = user.HasEmailVerified, IsPhoneVerified = user.HasPhoneVerified, IsVerified = user.IsVerified });
			}
			catch (Exception e)
			{
				// LOGS //
				_logger.LogError(e, "Error κατα τη διαδικασία Login");
				return RequestHandlerTool.HandleInternalServerError(e, "POST");
			}
		}

		/// <summary>
		/// Συνάρτηση για την εκτέλεση αποσύνδεσης χρήστη
		/// </summary>
		[HttpPost("logout")]
		[Authorize]
		[ProducesResponseType(200)]
		[ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
		[ProducesResponseType(500, Type = typeof(String))]
		public IActionResult Logout()
		{
			String? authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();

			if (String.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
			{
				ModelState.AddModelError("error", "Απουσία σωστής γραφής αυθεντικοποίησης token");
				return BadRequest(ModelState);
			}

			String? token = authHeader.Substring("Bearer ".Length).Trim();

			if (String.IsNullOrEmpty(token))
			{
				ModelState.AddModelError("error", "Απουσία token αυθεντικοποιημένου χρήστη");
				return BadRequest(ModelState);
			}

			JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
			JwtSecurityToken jwtToken = handler.ReadJwtToken(token);

			String? tokenId = jwtToken.Id;
			if (String.IsNullOrEmpty(tokenId))
			{
				// LOGS //
				_logger.LogError("Αποτυχία εύρεσης id του token");
				return RequestHandlerTool.HandleInternalServerError(new Exception("Αποτυχία εύρεσης id του token"), "POST");
			}

			DateTime? expiration = jwtToken.ValidTo;
			if (!expiration.HasValue)
			{
				// LOGS //
				_logger.LogError("Αποτυχία εύρεσης ημερομηνίας λήξης του token");
				return RequestHandlerTool.HandleInternalServerError(new Exception("Αποτυχία εύρεσης ημερομηνίας λήξης του token"), "POST");
			}

			_jwtService.RevokeToken(tokenId, expiration.Value);

			return Ok();
		}

		[HttpGet("google/callback")]
		public IActionResult GoogleCallback() { return Ok(); }

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
			catch (InvalidDataException ide)
			{
				_logger.LogError(ide, "Error while registering unverified user");
				return RequestHandlerTool.HandleInternalServerError(ide, "POST", "/auth/register/unverified", ide.Message);

			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error while registering unverified user");
				return RequestHandlerTool.HandleInternalServerError(e, "POST");
			}
		}

		[HttpPost("register/unverified/google")]
		[ProducesResponseType(200, Type = typeof(String))]
		[ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
		[ProducesResponseType(500, Type = typeof(String))]
		public async Task<IActionResult> RegisterUserWithGoogleUnverified([FromBody] AuthPayload payload)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				User user = await _userService.GetGoogleUser(payload.ProviderAccessCode);
				UserDto model = _mapper.Map<UserDto>(user);
				return Ok(model);
			}
			catch (InvalidDataException ide)
			{
				_logger.LogError(ide, "Error while registering unverified user with google");
				return RequestHandlerTool.HandleInternalServerError(ide, "POST", "/auth/register/unverified/google", ide.Message);

			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error while registering unverified user with google");
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
		public async Task<IActionResult> SendOtp([FromBody] AuthPayload payload)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				await _userService.GenerateNewOtpAsync(payload.Phone);
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
		public async Task<IActionResult> VerifyUserOtp([FromBody] AuthPayload payload)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				if (!_userService.VerifyOtp(payload.Phone, payload.Otp))
				{
					// LOGS //
					_logger.LogError("Αποτυχία επιβεβαίωσης OTP");
					ModelState.AddModelError("error", $"Αποτυχία επιβεβαίωσης OTP: {payload.Otp}");
					return Unauthorized(ModelState);
				}

				User? verifyPhoneUser = null;
				if ((verifyPhoneUser = (await _userService.RetrieveUserAsync(payload.Id, payload.Email))) == null)
				{
					// LOGS //
					_logger.LogError("Failed to verify phonenumber. The user with this email or id was not found");
					ModelState.AddModelError("error", "Failed to verify phonenumber. The user with this email or id was not found");
					return BadRequest(ModelState);
				}

				verifyPhoneUser.HasPhoneVerified = true;

				UserDto persisted = await _userService.Persist(verifyPhoneUser, false, new() { nameof(UserDto.Id) });
				if (persisted == null)
				{
					// LOGS //
					_logger.LogError("Failed to verify phonenumber. The user with this email or id failed to update");

					return RequestHandlerTool.HandleInternalServerError(new Exception("Failed to verify phonenumber. The user with this email or id failed to update"), "POST");
				}

				await _userService.VerifyUserAsync(persisted.Id, null);

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
		public async Task<IActionResult> SendEmailVerification([FromBody] AuthPayload payload)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				await _userService.SendVerficationEmailAsync(payload.Email);
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
		public async Task<IActionResult> VerifyEmail([FromBody] AuthPayload payload)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				String identifiedEmail = _userService.VerifyEmail(payload.Token);
				if (String.IsNullOrEmpty(identifiedEmail))
				{
					// LOGS //
					_logger.LogError("Failed to verify email");
					ModelState.AddModelError("error", "Failed to verify email");
					return BadRequest(ModelState);
				}

				User? verifyEmailUser = null;
				if ((verifyEmailUser = (await _userService.RetrieveUserAsync(null, identifiedEmail))) == null)
				{
					// LOGS //
					_logger.LogError("Failed to verify email. The user with this email or id was not found");
					ModelState.AddModelError("error", "Failed to verify email. The user with this email or id was not found");
					return BadRequest(ModelState);
				}

				verifyEmailUser.HasEmailVerified = true;

				UserDto persisted = await _userService.Persist(verifyEmailUser, false, new() { nameof(UserDto.Id), nameof(UserDto.Role) });
				if (persisted == null)
				{
					// LOGS //
					_logger.LogError("Failed to verify email. The user with this email or id failed to update");

					return RequestHandlerTool.HandleInternalServerError(new Exception("Failed to verify email. The user with this email or id failed to update"), "POST");
				}

				await _userService.VerifyUserAsync(verifyEmailUser.Id, null);

				return Ok(persisted);
			}

			catch (Exception e)
			{
				// LOGS //
				_logger.LogError(e, "Error while verifying email");
				return RequestHandlerTool.HandleInternalServerError(e, "POST");
			}
		}

		/// <summary>
		/// Επιβεβαίωση email χρήστη.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpPost("verify-user")]
		[ProducesResponseType(200)]
		[ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
		[ProducesResponseType(500, Type = typeof(String))]
		public async Task<IActionResult> VerifyUser([FromBody] AuthPayload payload)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				if (!(await _userService.VerifyUserAsync(payload.Id, payload.Email)))
				{
					// LOGS //
					_logger.LogError("Failed to verify user");
					ModelState.AddModelError("error", "Failed to verify email");
					return BadRequest(ModelState);
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
		/// Αποστολή email επαναφοράς κωδικού πρόσβασης.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpPost("send/reset-password")]
		[ProducesResponseType(200)]
		[ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
		[ProducesResponseType(500, Type = typeof(String))]
		public async Task<IActionResult> SendResetPasswordEmail([FromBody] AuthPayload AuthPayload)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				// Αποστολή reset-password email για έναρξη επαναφοράς κωδικού
				await _userService.SendResetPasswordEmailAsync(AuthPayload.Email);
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
		/// Επιβεβαίωση email χρήστη.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpPost("verify-reset-password-token")]
		[ProducesResponseType(200)]
		[ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
		[ProducesResponseType(500, Type = typeof(String))]
		public async Task<IActionResult> VerifyResetPasswordToken([FromBody] AuthPayload payload)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				String identifiedEmail = await _userService.VerifyResetPasswordToken(payload.Token);
				if (String.IsNullOrEmpty(identifiedEmail))
				{
					// LOGS //
					_logger.LogError("Failed to verify token");
					ModelState.AddModelError("error", "Failed to verify token");
					return BadRequest(ModelState);
				}


				return Ok(new UserDto()
				{
					Email = identifiedEmail,
				});
			}

			catch (Exception e)
			{
				// LOGS //
				_logger.LogError(e, "Error while verifying email");
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
		public async Task<IActionResult> ResetPassword([FromBody] AuthPayload AuthPayload)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				// Reset the password
				if (!await _userService.ResetPasswordAsync(AuthPayload.Email, AuthPayload.Password))
				{
					// LOGS //
					_logger.LogError("Αποτυχία επαναφοράς κωδικού");
					ModelState.AddModelError("error", "Αποτυχία επαναφοράς κωδικού");
					return BadRequest(ModelState);
				}

				// Redirect στο login page που βρίσκεται στο : /auth/login αν είναι επιτυχής η επαναφορά
				return Ok();
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
