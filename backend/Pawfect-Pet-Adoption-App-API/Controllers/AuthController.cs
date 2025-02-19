namespace Pawfect_Pet_Adoption_App_API.Controllers
{
	using Microsoft.AspNetCore.Authorization;
	using Microsoft.AspNetCore.Mvc;

	using Pawfect_Pet_Adoption_App_API.Data.Entities;
	using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
	using Pawfect_Pet_Adoption_App_API.DevTools;
	using Pawfect_Pet_Adoption_App_API.Models;
	using Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices;
	using Pawfect_Pet_Adoption_App_API.Services.UserServices;

	using System.IdentityModel.Tokens.Jwt;

	[ApiController]
	[Route("api/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly IUserService _userService;
		private readonly ILogger<AuthController> _logger;
		private readonly JwtService _jwtService;
		private readonly IAuthService _authService;

		public AuthController(IUserService userService, ILogger<AuthController> logger
			, JwtService jwtService, IAuthService authService)
		{
			_userService = userService;
			_logger = logger;
			_jwtService = jwtService;
			_authService = authService;
		}

		/// <summary>
		/// Συνάρτηση για την εκτέλεση σύνδεσης χρήστη
		/// </summary>
		[HttpPost("login")]
		[ProducesResponseType(200)]
		[ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
		[ProducesResponseType(401, Type = typeof(String))]
		[ProducesResponseType(500, Type = typeof(String))]
		public async Task<IActionResult> Login([FromBody] LoginPayload loginPayload)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				String? loginEmail = loginPayload.Email;
				String? loginCredential = loginPayload.Password;

				if (!String.IsNullOrEmpty(loginPayload.ProviderAccessCode) && String.IsNullOrEmpty(loginPayload.Password))
				{
					switch (loginPayload.LoginProvider)
					{
						case AuthProvider.Local:
							// LOGS //
							_logger.LogError("Λάθος provider \"Local\" με χρήση Auth Provider κωδικού");
							ModelState.AddModelError("error", "Λάθος provider \"Local\" με χρήση Auth Provider κωδικού");
							return BadRequest(ModelState);
						case AuthProvider.Google:
							(loginEmail, loginCredential) = await _authService.RetrieveGoogleCredentials(loginPayload.ProviderAccessCode);
							break;
					}
				}

				User? user = await _userService.RetrieveUserAsync(null, loginEmail);

				if (user == null)
				{
					return Unauthorized("Λάθος email χρήστη");
				}

				String? toCheckCredential = (user.AuthProvider == AuthProvider.Local) ? user.Password : user.AuthProviderId;

				if (!Security.ValidatedHashedValues(loginCredential, toCheckCredential))
				{
					return Unauthorized("Λάθος credentials χρήστη");
				}

				if (!user.IsVerified)
				{
					return Unauthorized("Ο χρήστης δεν έχει επιβεβαιώσει τα στοιχεία του.");
				}

				String? token = _jwtService.GenerateJwtToken(user.Id, user.Email, user.Role.ToString());
				if (token == null)
				{
					// LOGS //
					_logger.LogError("Αποτυχία παραγωγής JWT Token στο Login");
					return RequestHandlerTool.HandleInternalServerError(new InvalidOperationException("Αποτυχία παραγωγής JWT Token"), "POST");
				}

				return Ok(new { Token = token, Role = user.Role.ToString() });
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

	}
}
