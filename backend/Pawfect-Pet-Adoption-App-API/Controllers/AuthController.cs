namespace Pawfect_Pet_Adoption_App_API.Controllers
{
	using AutoMapper;
	using Microsoft.AspNetCore.Authorization;
	using Microsoft.AspNetCore.Mvc;
	using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
	using Pawfect_Pet_Adoption_App_API.DevTools;
    using Pawfect_Pet_Adoption_App_API.Exceptions;
    using Pawfect_Pet_Adoption_App_API.Models;
	using Pawfect_Pet_Adoption_App_API.Models.Authorization;
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
		private readonly IAuthenticationService _authService;
		private readonly IMapper _mapper;

        public AuthController(
			IUserService userService, ILogger<AuthController> logger
			, JwtService jwtService, IAuthenticationService authService
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
		public async Task<IActionResult> Login([FromBody] AuthPayload payload)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			String? loginEmail = payload.Email;
			String? loginCredential = payload.Password;

			if (payload.LoginProvider == AuthProvider.Google)
				(loginEmail, loginCredential) = await _userService.RetrieveGoogleCredentials(payload.ProviderAccessCode);

            Data.Entities.User? user = await _userService.RetrieveUserAsync(null, loginEmail);

			String? toCheckCredential = _userService.ExtractUserCredential(user);

			if (!Security.ValidatedHashedValues(loginCredential, toCheckCredential))
				throw new ForbiddenException("Invalid user credentials");

			if (!user.HasPhoneVerified)
				throw new ForbiddenException("Phone number has not been verified");

            String? token = _jwtService.
							GenerateJwtToken
							(
								user.Id, 
								user.Email, 
								[..user.Roles.Select(roleEnum => roleEnum.ToString())], 
								user.HasEmailVerified.ToString(), 
								user.IsVerified.ToString()
							);

			if (String.IsNullOrEmpty(token))
				throw new InvalidOperationException("Failed to create token");


			return Ok(
						new LoggedAccount() 
						{   Token = token, 
							Email = user.Email, 
							Phone = user.Phone, 
							Roles = user.Roles, 
							LoggedAt = DateTime.UtcNow, 
							IsEmailVerified = user.HasEmailVerified, 
							IsPhoneVerified = user.HasPhoneVerified, 
							IsVerified = user.IsVerified 
						}
					);
		}

		/// <summary>
		/// Συνάρτηση για την εκτέλεση αποσύνδεσης χρήστη
		/// </summary>
		[HttpPost("logout")]
		[Authorize]
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
				throw new ArgumentException("Failed to find user id on authentication token");

			DateTime? expiration = jwtToken.ValidTo;
			if (!expiration.HasValue)
                throw new ArgumentException("Failed to find expiration date on authentication token");

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
		public async Task<IActionResult> RegisterUserUnverified([FromBody] RegisterPersist toRegisterUser, [FromQuery] List<String> fields)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			return Ok(await _userService.RegisterUserUnverifiedAsync(toRegisterUser, fields));
		}

		[HttpPost("register/unverified/google")]
		public async Task<IActionResult> RegisterUserWithGoogleUnverified([FromBody] AuthPayload payload)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

            Data.Entities.User user = await _userService.GetGoogleUser(payload.ProviderAccessCode);
			Models.User.User model = _mapper.Map<Models.User.User>(user);
			return Ok(model);
		}

		/// <summary>
		/// Αποστολή OTP στον αριθμό τηλεφώνου του χρήστη.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpPost("send/otp")]
		public async Task<IActionResult> SendOtp([FromBody] AuthPayload payload)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			await _userService.GenerateNewOtpAsync(payload.Phone);
			return Ok();
		}

		/// <summary>
		/// Επιβεβαίωση OTP χρήστη.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpPost("verify-otp")]
		public async Task<IActionResult> VerifyUserOtp([FromBody] AuthPayload payload)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			if (!_userService.VerifyOtp(payload.Phone, payload.Otp))
			{
				// LOGS //
				_logger.LogError("Αποτυχία επιβεβαίωσης OTP");
				ModelState.AddModelError("error", $"Αποτυχία επιβεβαίωσης OTP: {payload.Otp}");
				return Unauthorized(ModelState);
			}

            Data.Entities.User? verifyPhoneUser = null;
			if ((verifyPhoneUser = (await _userService.RetrieveUserAsync(payload.Id, payload.Email))) == null)
			{
				// LOGS //
				_logger.LogError("Failed to verify phonenumber. The user with this email or id was not found");
				ModelState.AddModelError("error", "Failed to verify phonenumber. The user with this email or id was not found");
				return BadRequest(ModelState);
			}

			// ** VERIFY ** //
			verifyPhoneUser.HasPhoneVerified = true;
            Models.User.User persisted = await _userService.Persist(verifyPhoneUser, false, [nameof(Models.User.User.Id)]);

			await _userService.VerifyUserAsync(persisted.Id, null);

			return Ok();
		}

		/// <summary>
		/// Αποστολή επιβεβαίωσης email.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpPost("send/email-verification")]
		public async Task<IActionResult> SendEmailVerification([FromBody] AuthPayload payload)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			await _userService.SendVerficationEmailAsync(payload.Email);
			return Ok();
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
			if (!ModelState.IsValid) return BadRequest(ModelState);

			String identifiedEmail = _userService.VerifyEmail(payload.Token);
			if (String.IsNullOrEmpty(identifiedEmail))
			{
				_logger.LogError("Failed to verify email");
				ModelState.AddModelError("error", "Failed to verify email");
				return BadRequest(ModelState);
			}

            Data.Entities.User verifyEmailUser = null;
			if ((verifyEmailUser = (await _userService.RetrieveUserAsync(null, identifiedEmail))) == null)
			{
				_logger.LogError("Failed to verify email. The user with this email or id was not found");
				ModelState.AddModelError("error", "Failed to verify email. The user with this email or id was not found");
				return BadRequest(ModelState);
			}

            // ** VERIFY ** //
            verifyEmailUser.HasEmailVerified = true;
            Models.User.User persisted = await _userService.Persist(verifyEmailUser, false, [nameof(Models.User.User.Id), nameof(Models.User.User.Roles)]);

			await _userService.VerifyUserAsync(verifyEmailUser.Id, null);

			return Ok(persisted);
		}

		/// <summary>
		/// Επιβεβαίωση email χρήστη.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpPost("verify-user")]
		public async Task<IActionResult> VerifyUser([FromBody] AuthPayload payload)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			if (!(await _userService.VerifyUserAsync(payload.Id, payload.Email)))
				throw new ForbiddenException("User did not meet the requirements to be verified");

			return Ok();
		}

		/// <summary>
		/// Αποστολή email επαναφοράς κωδικού πρόσβασης.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpPost("send/reset-password")]
		public async Task<IActionResult> SendResetPasswordEmail([FromBody] AuthPayload AuthPayload)
		{
            if (!ModelState.IsValid)  return BadRequest(ModelState);

            // Αποστολή reset-password email για έναρξη επαναφοράς κωδικού
            await _userService.SendResetPasswordEmailAsync(AuthPayload.Email);

			return Ok();
		}

		/// <summary>
		/// Επιβεβαίωση email χρήστη.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpPost("verify-reset-password-token")]
		public async Task<IActionResult> VerifyResetPasswordToken([FromBody] AuthPayload payload)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			String identifiedEmail = await _userService.VerifyResetPasswordToken(payload.Token);
			if (String.IsNullOrEmpty(identifiedEmail))
			{
				// LOGS //
				_logger.LogError("Failed to verify token");
				ModelState.AddModelError("error", "Failed to verify token");
				return BadRequest(ModelState);
			}


			return Ok( new Models.User.User() { Email = identifiedEmail } );
		}

		/// <summary>
		/// Επαναφορά κωδικού πρόσβασης χρήστη.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpPost("reset-password")]
		public async Task<IActionResult> ResetPassword([FromBody] AuthPayload AuthPayload)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			// Reset the password
			if (!await _userService.ResetPasswordAsync(AuthPayload.Email, AuthPayload.Password))
			{
				// LOGS //
				_logger.LogError("Αποτυχία επαναφοράς κωδικού");
				ModelState.AddModelError("error", "Αποτυχία επαναφοράς κωδικού");
				return BadRequest(ModelState);
			}

			return Ok();
		}
	}
}
