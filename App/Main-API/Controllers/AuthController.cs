namespace Main_API.Controllers 
{ 
	using AutoMapper;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Main_API.Censors;
    using Main_API.Data.Entities;
    using Main_API.Data.Entities.EnumTypes;
	using Main_API.DevTools;
    using Main_API.Exceptions;
    using Main_API.Models;
	using Main_API.Models.Authorization;
    using Main_API.Query.Interfaces;
    using Main_API.Services.AuthenticationServices;
    using Main_API.Services.Convention;
    using Main_API.Services.CookiesServices;
    using Main_API.Services.UserServices;
    using Main_API.Transactions;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;

	[ApiController]
	[Route("auth")]
	public class AuthController : ControllerBase
	{
		private readonly IUserService _userService;
		private readonly ILogger<AuthController> _logger;
		private readonly JwtService _jwtService;
		private readonly Services.AuthenticationServices.IAuthenticationService _authService;
		private readonly IMapper _mapper;
        private readonly PermissionPolicyProvider _permissionPolicyProvider;
        private readonly IAuthorizationContentResolver _authorizationContentResolver;
        private readonly ClaimsExtractor _claimsExtractor;
        private readonly IConventionService _conventionService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ICookiesService _cookiesService;

        public AuthController
		(
			IUserService userService, 
			ILogger<AuthController> logger, 
			JwtService jwtService, 
			Services.AuthenticationServices.IAuthenticationService authService,
			IMapper mapper, 
			PermissionPolicyProvider permissionPolicyProvider,
			IAuthorizationContentResolver AuthorizationContentResolver, 
			ClaimsExtractor claimsExtractor,
			IConventionService conventionService, 
			IRefreshTokenRepository refreshTokenRepository,
			ICookiesService cookiesService
        )
		{
			_userService = userService;
			_logger = logger;
			_jwtService = jwtService;
			_authService = authService;
			_mapper = mapper;
            _permissionPolicyProvider = permissionPolicyProvider;
            _authorizationContentResolver = AuthorizationContentResolver;
            _claimsExtractor = claimsExtractor;
            _conventionService = conventionService;
            _refreshTokenRepository = refreshTokenRepository;
            _cookiesService = cookiesService;
        }

		/// <summary>
		/// Συνάρτηση για την εκτέλεση σύνδεσης χρήστη
		/// </summary>
		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] AuthPayload payload)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			String loginEmail = payload.Email;
			String loginCredential = payload.Password;

			if (payload.LoginProvider == AuthProvider.Google)
				(loginEmail, loginCredential) = await _userService.RetrieveGoogleCredentials(payload.ProviderAccessCode);

            Data.Entities.User user = await _userService.RetrieveUserAsync(null, loginEmail);
			if (user == null) throw new NotFoundException();


			String toCheckCredential = _userService.ExtractUserCredential(user);

			if (!Security.ValidatedHashedValues(loginCredential, toCheckCredential))
				throw new ForbiddenException("Invalid user credentials");

			if (!user.HasPhoneVerified)
				throw new ForbiddenException("Phone number has not been verified");

            List<String> userRoles = [.. user.Roles.Select(roleEnum => roleEnum.ToString())];
            List<String> permissions = [.. _permissionPolicyProvider.GetPermissionsAndAffiliatedForRoles(userRoles)];

            // Delete cookies (in case there where any)
            _cookiesService.DeleteCookie(JwtService.ACCESS_TOKEN);
            _cookiesService.DeleteCookie(JwtService.REFRESH_TOKEN);

            // ** TOKENS ** //
            String? token = _jwtService.GenerateJwtToken(user.Id, user.Email, userRoles, user.IsVerified);
			if (String.IsNullOrEmpty(token))
				throw new InvalidOperationException("Failed to create token");

            RefreshToken refreshToken = _jwtService.GenerateRefreshToken(user.Id, HttpContext.Connection.RemoteIpAddress?.ToString());
            if (String.IsNullOrEmpty(await _refreshTokenRepository.AddAsync(refreshToken)))
                throw new InvalidOperationException("Failed to create refresh token");

			// ** COOKIES ** //
			_cookiesService.SetCookie(JwtService.ACCESS_TOKEN, token, DateTime.UtcNow.AddMinutes(_jwtService.JwtExpireAfterMinutes));
            _cookiesService.SetCookie(JwtService.REFRESH_TOKEN, refreshToken.Token, refreshToken.ExpiresAt);

			// ** ACCOUNT SERVE ** //
            return Ok(
				new LoggedAccount() 
				{   
					Email = user.Email,
                    Phone = user.Phone, 
					Roles = userRoles, 
					Permissions = permissions,
					LoggedAt = DateTime.UtcNow, 
					IsEmailVerified = user.HasEmailVerified, 
					IsPhoneVerified = user.HasPhoneVerified, 
					IsVerified = user.IsVerified 
				}
			);
		}

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken()
        {
            String? refreshTokenString = _cookiesService.GetCookie(JwtService.REFRESH_TOKEN);
            if (String.IsNullOrEmpty(refreshTokenString))
                return BadRequest("Refresh token is missing");

            RefreshToken refreshToken = await _refreshTokenRepository.FindAsync(rt => rt.Token == refreshTokenString);
            if (refreshToken == null || refreshToken.ExpiresAt < DateTime.UtcNow)
                return BadRequest("Invalid or expired refresh token");

            // ** ACCOUNT ** //
            String userId = refreshToken.LinkedTo;
            Data.Entities.User user = await _userService.RetrieveUserAsync(userId, null);
            if (user == null) return NotFound("User not found");

            List<String> userRoles = [.. user.Roles.Select(roleEnum => roleEnum.ToString())];

            String newAccessToken = _jwtService.GenerateJwtToken(user.Id, user.Email, userRoles, user.IsVerified);
            if (String.IsNullOrEmpty(newAccessToken))
                throw new InvalidOperationException("Failed to create token");

            // ** COOKIES ** //
            _cookiesService.DeleteCookie(JwtService.ACCESS_TOKEN);
            _cookiesService.SetCookie(JwtService.ACCESS_TOKEN, newAccessToken, DateTime.UtcNow.AddMinutes(_jwtService.JwtExpireAfterMinutes));

            return Ok(new LoggedAccount
            {
                Email = user.Email,
                Phone = user.Phone,
                Roles = userRoles,
                Permissions = [.. _permissionPolicyProvider.GetPermissionsAndAffiliatedForRoles(userRoles)],
                LoggedAt = DateTime.UtcNow,
                IsEmailVerified = user.HasEmailVerified,
                IsPhoneVerified = user.HasPhoneVerified,
                IsVerified = user.IsVerified
            });
        }

        /// <summary>
        /// Συνάρτηση για την εκτέλεση αποσύνδεσης χρήστη
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            // Extract access token from cookie
            String? token = _cookiesService.GetCookie(JwtService.ACCESS_TOKEN);
            if (String.IsNullOrEmpty(token)) return Unauthorized("Access token is missing");

            // Validate and parse the token
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken;
            try
            {
                jwtToken = handler.ReadJwtToken(token);
            }
            catch (Exception)
            {
                return Unauthorized("Invalid access token");
            }

            // Extract token ID (jti)
            String? tokenId = jwtToken.Id;
            if (String.IsNullOrEmpty(tokenId))
                return Unauthorized("Failed to find token ID in access token");

            // Extract expiration
            DateTime? expiration = jwtToken.ValidTo;
            if (!expiration.HasValue)
                return Unauthorized("Failed to find expiration date in access token");

            // Revoke the token
            _jwtService.RevokeToken(tokenId, expiration.Value);
				
            // Handle refresh token deletion
            String? refreshTokenString = _cookiesService.GetCookie(JwtService.REFRESH_TOKEN);
            if (!String.IsNullOrEmpty(refreshTokenString))
            {
                RefreshToken refreshToken = await _refreshTokenRepository.FindAsync(rt => rt.Token == refreshTokenString);
                if (refreshToken != null) await _refreshTokenRepository.DeleteAsync(refreshToken);

				// Delete cookies
				_cookiesService.DeleteCookie(JwtService.ACCESS_TOKEN);
                _cookiesService.DeleteCookie(JwtService.REFRESH_TOKEN);
            }

            return Ok();
        }

        [HttpPost("me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            // Check if user is authenticated
            if (!User.Identity?.IsAuthenticated ?? true)
                throw new ForbiddenException("User is not authenticated");

			ClaimsPrincipal currentUser = _authorizationContentResolver.CurrentPrincipal();
            String userId = _claimsExtractor.CurrentUserId(currentUser);
            if (!_conventionService.IsValidId(userId)) throw new UnAuthenticatedException("User is not authenticated.");

            // ** TOKENS ** //	
            String? token = _cookiesService.GetCookie(JwtService.ACCESS_TOKEN);
            if (String.IsNullOrEmpty(token))
                throw new UnAuthenticatedException("No access token found for the user");

            // ** ACCOUNT DATA ** //
            // Retrieve user from database for additional details
            Data.Entities.User user = await _userService.RetrieveUserAsync(userId, null);
            if (user == null)
                throw new NotFoundException("Authorized user not found");

            // Get roles and permissions
            List<String> userRoles = user.Roles?.Select(roleEnum => roleEnum.ToString()).ToList() ?? new List<String>();
            List<String> permissions = _permissionPolicyProvider.GetPermissionsAndAffiliatedForRoles(userRoles).ToList();
			DateTime loggedAt = _claimsExtractor.CurrentUserLoggedAtDate(currentUser);

            // Return LoggedAccount
            return Ok(new LoggedAccount()
            {
                Email = user.Email,
                Phone = user.Phone,
                Roles = userRoles,
                Permissions = permissions,
                LoggedAt = loggedAt,
                IsEmailVerified = user.HasEmailVerified,
                IsPhoneVerified = user.HasPhoneVerified,
                IsVerified = user.IsVerified
            });
        }

        [HttpGet("google/callback")]
		public IActionResult GoogleCallback() { return Ok(); }

		/// <summary>
		/// Εγγραφή μη επιβεβαιωμένου χρήστη.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpPost("register/unverified")]
        [ServiceFilter(typeof(MongoTransactionFilter))]
		public async Task<IActionResult> RegisterUserUnverified([FromBody] RegisterPersist toRegisterUser, [FromQuery] List<String> fields)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			fields = BaseCensor.PrepareFieldsList(fields);

            return Ok(await _userService.RegisterUserUnverifiedAsync(toRegisterUser, fields));
		}

		[HttpPost("register/unverified/google")]
        [ServiceFilter(typeof(MongoTransactionFilter))]
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
        [ServiceFilter(typeof(MongoTransactionFilter))]
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
        [ServiceFilter(typeof(MongoTransactionFilter))]
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
        [ServiceFilter(typeof(MongoTransactionFilter))]
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
        [ServiceFilter(typeof(MongoTransactionFilter))]
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
