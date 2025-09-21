using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawfect_API.Censors;
using Pawfect_API.Data.Entities;
using Pawfect_API.Data.Entities.EnumTypes;
using Pawfect_API.DevTools;
using Pawfect_API.Exceptions;
using Pawfect_API.Models;
using Pawfect_API.Models.Authorization;
using Pawfect_API.Query.Interfaces;
using Pawfect_API.Services.AuthenticationServices;
using Pawfect_API.Services.Convention;
using Pawfect_API.Services.CookiesServices;
using Pawfect_API.Services.UserServices;
using Pawfect_API.Transactions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;
using Pawfect_API.Services.UserServices;
using Pawfect_API.Models.UserAvailability;
using Pawfect_API.Models.Authorization;
using Pawfect_API.Attributes;
using Pawfect_API.Data.Entities.EnumTypes;

namespace Pawfect_API.Controllers 
{ 
    [ApiController]
	[Route("auth")]
    [RateLimit(RateLimitLevel.Moderate)]
    public class AuthController : ControllerBase
	{
		private readonly IUserService _userService;
		private readonly ILogger<AuthController> _logger;
		private readonly JwtService _jwtService;
		private readonly IMapper _mapper;
        private readonly PermissionPolicyProvider _permissionPolicyProvider;
        private readonly IAuthorizationContentResolver _authorizationContentResolver;
        private readonly ClaimsExtractor _claimsExtractor;
        private readonly IMemoryCache _memoryCache;
        private readonly IUserAvailabilityService _userAvailabilityService;
        private readonly IConventionService _conventionService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ICookiesService _cookiesService;

        public AuthController
		(
			IUserService userService, 
			ILogger<AuthController> logger, 
			JwtService jwtService, 
			IMapper mapper, 
			PermissionPolicyProvider permissionPolicyProvider,
			IAuthorizationContentResolver AuthorizationContentResolver, 
			ClaimsExtractor claimsExtractor,
            IMemoryCache memoryCache,
            IUserAvailabilityService userAvailabilityService,
			IConventionService conventionService, 
			IRefreshTokenRepository refreshTokenRepository,
			ICookiesService cookiesService
        )
		{
			_userService = userService;
			_logger = logger;
			_jwtService = jwtService;
			_mapper = mapper;
            _permissionPolicyProvider = permissionPolicyProvider;
            _authorizationContentResolver = AuthorizationContentResolver;
            _claimsExtractor = claimsExtractor;
            _memoryCache = memoryCache;
            _userAvailabilityService = userAvailabilityService;
            _conventionService = conventionService;
            _refreshTokenRepository = refreshTokenRepository;
            _cookiesService = cookiesService;
        }

		// ** AUTHENTICATION ** //

		[HttpPost("login")]
        [RateLimit(RateLimitLevel.Restrictive)]
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

            LoggedAccount loggedAccount = new LoggedAccount()
            {
                UserId = user.Id,
                ShelterId = user.ShelterId,
                Email = user.Email,
                Phone = user.Phone,
                Roles = userRoles,
                Permissions = permissions,
                LoggedAt = DateTime.UtcNow,
                IsEmailVerified = user.HasEmailVerified,
                IsPhoneVerified = user.HasPhoneVerified,
                IsVerified = user.IsVerified
            };

            _memoryCache.Set($"User_Account_{user.Id}", JsonHelper.SerializeObjectFormatted(loggedAccount), TimeSpan.FromMinutes(_jwtService.JwtExpireAfterMinutes));

            // ** ACCOUNT SERVE ** //
            return Ok(loggedAccount);
		}

        [HttpPost("refresh")]
        [RateLimit(RateLimitLevel.Restrictive)]
        public async Task<IActionResult> RefreshToken()
        {
            String? refreshTokenString = _cookiesService.GetCookie(JwtService.REFRESH_TOKEN);
            if (String.IsNullOrEmpty(refreshTokenString)) return Forbid("Refresh token is missing");

            RefreshToken refreshToken = await _refreshTokenRepository.FindAsync(rt => rt.Token.Equals(refreshTokenString) && rt.ExpiresAt > DateTime.UtcNow);
            if (refreshToken == null) return Forbid("Invalid or expired refresh token");

            // ** ACCOUNT ** //
            String userId = refreshToken.LinkedTo;
            Data.Entities.User user = await _userService.RetrieveUserAsync(userId, null);
            if (user == null) return NotFound("User not found");


            List<String> userRoles = [.. user.Roles.Select(roleEnum => roleEnum.ToString())];

            String newAccessToken = _jwtService.GenerateJwtToken(user.Id, user.Email, userRoles, user.IsVerified);
            if (String.IsNullOrEmpty(newAccessToken)) throw new Exception("Failed to create token");

            // ** COOKIES ** //
            _cookiesService.DeleteCookie(JwtService.ACCESS_TOKEN);

            _cookiesService.SetCookie(JwtService.ACCESS_TOKEN, newAccessToken, DateTime.UtcNow.AddMinutes(_jwtService.JwtExpireAfterMinutes));

            LoggedAccount loggedAccount = new LoggedAccount
            {
                UserId = user.Id,
                ShelterId = user.ShelterId,
                Email = user.Email,
                Phone = user.Phone,
                Roles = userRoles,
                Permissions = [.. _permissionPolicyProvider.GetPermissionsAndAffiliatedForRoles(userRoles)],
                LoggedAt = DateTime.UtcNow,
                IsEmailVerified = user.HasEmailVerified,
                IsPhoneVerified = user.HasPhoneVerified,
                IsVerified = user.IsVerified
            };

            _memoryCache.Remove($"User_Account_{user.Id}");
            _memoryCache.Set($"User_Account_{user.Id}", JsonHelper.SerializeObjectFormatted(loggedAccount), TimeSpan.FromMinutes(_jwtService.JwtExpireAfterMinutes));

            return Ok(loggedAccount);
        }

        [HttpPost("logout")]
        [Authorize]
        [RateLimit(RateLimitLevel.Restrictive)]
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

            LoggedAccount loggedAccount = JsonHelper.DeserializeObjectFormattedSafe<LoggedAccount>(_memoryCache.Get<String>($"User_Account_{userId}"));
            if (loggedAccount != null) return Ok(loggedAccount);

            // ** TOKENS ** //	
            String? token = _cookiesService.GetCookie(JwtService.ACCESS_TOKEN);
            if (String.IsNullOrEmpty(token))
                throw new UnAuthenticatedException("No access token found for the user");

            // ** ACCOUNT DATA ** //
            // Retrieve user from database for additional details
            Data.Entities.User user = await _userService.RetrieveUserAsync(userId, null);
            if (user == null)
                throw new ForbiddenException("Authorized user not found");

            // Get roles and permissions
            List<String> userRoles = user.Roles?.Select(roleEnum => roleEnum.ToString()).ToList() ?? new List<String>();
            List<String> permissions = _permissionPolicyProvider.GetPermissionsAndAffiliatedForRoles(userRoles).ToList();
			DateTime loggedAt = _claimsExtractor.CurrentUserLoggedAtDate(currentUser);

            // Return LoggedAccount
            return Ok(new LoggedAccount()
            {
                UserId = user.Id,
                ShelterId = user.ShelterId,
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


		// ** REGISTER ** //

		[HttpPost("register/unverified")]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        [RateLimit(RateLimitLevel.Strict)]
        public async Task<IActionResult> RegisterUserUnverified([FromBody] RegisterPersist toRegisterUser, [FromQuery] List<String> fields)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			fields = BaseCensor.PrepareFieldsList(fields);

            Models.User.User user = await _userService.RegisterUserUnverifiedAsync(toRegisterUser, fields);
            if (user == null) throw new InvalidOperationException("Failed to save user . Please try again later.");

            _userAvailabilityService.InvalidateAvailabilityCache(toRegisterUser.User.Email, toRegisterUser.User.Phone);

            return Ok(user);
		}

		[HttpPost("register/unverified/google")]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        [RateLimit(RateLimitLevel.Strict)]
        public async Task<IActionResult> RegisterUserWithGoogleUnverified([FromBody] AuthPayload payload)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

            Data.Entities.User user = await _userService.GetGoogleUser(payload.ProviderAccessCode);
			Models.User.User model = _mapper.Map<Models.User.User>(user);
			return Ok(model);
		}

        [HttpPost("check-availability")]
        public async Task<IActionResult> CheckUserAvailability([FromBody] UserAvailabilityCheck payload)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            UserAvailabilityResult result = await _userAvailabilityService.CheckUserAvailabilityAsync(payload);

            return Ok(result);
        }

        [HttpPost("send/otp")]
        [RateLimit(RateLimitLevel.Restrictive)]
        public async Task<IActionResult> SendOtp([FromBody] AuthPayload payload)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			await _userService.SendOtpAsync(payload.Phone, payload.Id);
			return Ok();
		}

		[HttpPost("verify-otp")]
        [RateLimit(RateLimitLevel.Restrictive)]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> VerifyUserOtp([FromBody] AuthPayload payload)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			if (!_userService.VerifyOtp(payload.Phone, payload.Otp))
			{
				_logger.LogError("Αποτυχία επιβεβαίωσης OTP");
				ModelState.AddModelError("error", $"Αποτυχία επιβεβαίωσης OTP: {payload.Otp}");
				return Unauthorized(ModelState);
			}

            Data.Entities.User? verifyPhoneUser = null;
			if ((verifyPhoneUser = (await _userService.RetrieveUserAsync(payload.Id, payload.Email))) == null)
			{
				_logger.LogError("Failed to verify phonenumber. The user with this email or id was not found");
				ModelState.AddModelError("error", "Failed to verify phonenumber. The user with this email or id was not found");
				return BadRequest(ModelState);
			}

			// ** VERIFY ** //
			verifyPhoneUser.HasPhoneVerified = true;
            Models.User.User persisted = await _userService.Persist(verifyPhoneUser, false, [nameof(Models.User.User.Id)]);

            await _userService.VerifyUserAsync(verifyPhoneUser.Id, null);

            return Ok();
		}

		[HttpPost("send/email-verification")]
        [RateLimit(RateLimitLevel.Restrictive)]
        public async Task<IActionResult> SendEmailVerification([FromBody] AuthPayload payload)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			await _userService.SendVerficationEmailAsync(payload.Email);
			return Ok();
		}

		[HttpPost("verify-email")]
        [RateLimit(RateLimitLevel.Restrictive)]
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

            verifyEmailUser.HasEmailVerified = true;
            Models.User.User persisted = await _userService.Persist(verifyEmailUser, false, [nameof(Models.User.User.Id), nameof(Models.User.User.Roles)]);

            await _userService.VerifyUserAsync(verifyEmailUser.Id, null);

			return Ok(persisted);
		}

		[HttpPost("verify/user")]
        [RateLimit(RateLimitLevel.Strict)]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> VerifyUser([FromBody] AuthPayload payload)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

            if (!await _userService.VerifyUserAsync(payload.Id, payload.Email))
                return BadRequest();

			return Ok();
		}

        [HttpPost("verify/shelter")]
        [RateLimit(RateLimitLevel.Strict)]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> VerifyShelter([FromBody] AdminVerifyPayload payload)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (!await _userService.VerifyShelterAsync(payload))
                return BadRequest();

            return Ok();
        }

        [HttpPost("send/reset-password")]
        [RateLimit(RateLimitLevel.Restrictive)]
        public async Task<IActionResult> SendResetPasswordEmail([FromBody] AuthPayload AuthPayload)
		{
            if (!ModelState.IsValid)  return BadRequest(ModelState);

            await _userService.SendResetPasswordEmailAsync(AuthPayload.Email);

			return Ok();
		}

		[HttpPost("verify-reset-password-token")]
        [RateLimit(RateLimitLevel.Restrictive)]
        public async Task<IActionResult> VerifyResetPasswordToken([FromBody] AuthPayload payload)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			String identifiedEmail = await _userService.VerifyResetPasswordToken(payload.Token);
			if (String.IsNullOrEmpty(identifiedEmail))
			{
				_logger.LogError("Failed to verify token");
				ModelState.AddModelError("error", "Failed to verify token");
				return BadRequest(ModelState);
			}


			return Ok( new Models.User.User() { Email = identifiedEmail } );
		}

		[HttpPost("reset-password")]
        [RateLimit(RateLimitLevel.Restrictive)]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> ResetPassword([FromBody] AuthPayload AuthPayload)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			if (!await _userService.ResetPasswordAsync(AuthPayload.Email, AuthPayload.Password))
			{
				_logger.LogError("Αποτυχία επαναφοράς κωδικού");
				ModelState.AddModelError("error", "Αποτυχία επαναφοράς κωδικού");
				return BadRequest(ModelState);
			}

			return Ok();
		}

        // ** GOOGLE ** //
        [HttpGet("google/callback")]
        public IActionResult GoogleCallback() { return Ok(); }
    }
}
