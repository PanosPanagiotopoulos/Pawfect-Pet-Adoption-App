using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Models;
using Pawfect_Pet_Adoption_App_API.Models.User;
using Pawfect_Pet_Adoption_App_API.Services;

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
        /// Εγγραφή μη επιβεβαιωμένου χρήστη.
        /// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 string
        /// </summary>
        [HttpPost("register/unverified")]
        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(500, Type = typeof(string))]
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
        /// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 string
        /// </summary>
        [HttpPost("send/otp")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(500, Type = typeof(string))]
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
        /// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 string
        /// </summary>
        [HttpPost("verify-otp")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(500, Type = typeof(string))]
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

                if ((await _userService.PersistUserAsync(new UserPersist
                {
                    Id = otpPayload.Id ?? string.Empty,
                    Email = otpPayload.Email ?? string.Empty,
                    HasPhoneVerified = true
                })) is string userId && !string.IsNullOrEmpty(userId))
                {
                    return Ok();
                }

                // LOGS //
                _logger.LogError("Αποτυχία επιβεβαίωσης ενώς χρήστη OTP");
                return RequestHandlerTool.HandleInternalServerError(new Exception("Αποτυχία επιβεβαίωσης ενώς χρήστη OTP"), "POST");
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
        /// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 string
        /// </summary>
        [HttpPost("send/email-verification")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(500, Type = typeof(string))]
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
        /// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 string
        /// </summary>
        [HttpPost("verify-email")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(500, Type = typeof(string))]
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

                if ((await _userService.PersistUserAsync(new UserPersist
                {
                    Id = emailPayload.Id ?? string.Empty,
                    Email = emailPayload.Email ?? string.Empty,
                    HasEmailVerified = true
                })) is string userId && !string.IsNullOrEmpty(userId))
                {
                    return Ok();
                }

                // LOGS //
                _logger.LogError("Failed to verify email");
                return RequestHandlerTool.HandleInternalServerError(new Exception("Failed to verify email"), "POST");
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
        /// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 string
        /// </summary>
        [HttpPost("register/verified")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(500, Type = typeof(string))]
        public async Task<IActionResult> RegisterUserVerified([FromBody] RegisterPersist toRegisterUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                if (!(await _userService.VerifyUserAsync(toRegisterUser)))
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
        /// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 string
        /// </summary>
        [HttpPost("send/reset-password")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(500, Type = typeof(string))]
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
        /// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 string
        /// </summary>
        [HttpPost("reset-password")]
        [ProducesResponseType(302)]
        [ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
        [ProducesResponseType(500, Type = typeof(string))]
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