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

        public UserController(IUserService userService, ILogger<UserController> logger
            , IMemoryCache memoryCache)
        {
            _userService = userService;
            this._logger = logger;
            this._memoryCache = memoryCache;
        }
        /*
         Example Payload:
        
            {
              "user": {
                "email": "user@example.com",
                "password": "SecurePassword123!",
                "fullName": "John Doe",
                "role": 1,
                "phone": "+306943882441",
                "location": {
                  "address": "123 Main Street",
                  "number": "456",
                  "city": "San Francisco",
                  "zipCode": "94103"
                },
                "authProvider": 1,
                "authProviderId": null
              },
              "shelter": null
           }
         */
        [HttpPost("register/unverified")]
        public async Task<IActionResult> RegisterUserUnverified([FromBody] RegisterPersist toRegisterUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Αποθήκευση μη επιβεβαιωμένου χρήστη
                return Ok(await _userService.RegisterUserUnverifiedAsync(toRegisterUser));
            }
            catch (Exception e)
            {
                // LOGS //
                _logger.LogError(e, "Error κατασκευής μη επιβεβαιωμένου χρήστη");
                return RequestHandlerTool.HandleInternalServerError(e, "POST");
            }
        }

        [HttpPost("send/otp")]
        public async Task<IActionResult> SendOtp([FromBody] string? phonenumber)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Αποστέλουμε otp στον αριθμό τηλεφώνου και το αποθηκέυουμε στην cache
                await _userService.GenerateNewOtpAsync(phonenumber);

                return Ok();
            }
            catch (Exception e)
            {
                // LOGS //
                _logger.LogError(e, "Error στην αποστολή otp σε χρήστη");
                return RequestHandlerTool.HandleInternalServerError(e, "POST");
            }
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyUserOtp([FromBody] string? email, [FromBody] string? phonenumber, [FromBody] OTPVerification otpVerification)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Check αν το otp match-αρει με τον αριθμό τηλεφώνου
                if (!(_userService.VerifyOtp(phonenumber, otpVerification)))
                {
                    ModelState.AddModelError("error", $"Failed to verify otp : {otpVerification.Otp}");
                    return BadRequest(ModelState);
                }

                // Κάνουμε update τον χρήστη με verified αριθμό τηλεφώνου
                if ((await _userService.PersistUserAsync(new UserPersist
                {
                    Email = email,
                    HasPhoneVerified = true
                })) is string userId && !string.IsNullOrEmpty(userId)
                )
                {
                    return Ok();
                }

                _logger.LogError("Error στην επιβεβαίωση OTP χρήστη");
                return RequestHandlerTool.HandleInternalServerError(new Exception("Error στην επιβεβαίωση OTP χρήστη"), "POST");
            }
            catch (Exception e)
            {
                // LOGS //
                _logger.LogError(e, "Error στην επιβεβαίωση OTP του χρήστη");
                return RequestHandlerTool.HandleInternalServerError(e, "POST");
            }
        }

        [HttpPost("send/email-verification")]
        public async Task<IActionResult> SendEmailVerification([FromBody] string? email)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Αποστέλουμε το email επιβεβαίωσης και αποθηκέυουμε στην cache το token
                await _userService.SendVerficationEmailAsync(email);

                return Ok();
            }
            catch (Exception e)
            {
                // LOGS //
                _logger.LogError(e, "Προσπαθόντας να στείλουμε email επιβεβαίωσης στον χρήστη");
                return RequestHandlerTool.HandleInternalServerError(e, "POST");
            }
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] string? email, [FromBody] string? token)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Check αν το token που στάλθηκε ισχύει
                if (!(_userService.VerifyEmail(email, token)))
                {
                    ModelState.AddModelError("error", "Failed to verify email");
                    return BadRequest(ModelState);
                }

                // Κάνουμε update τον χρήστη με verified αριθμό τηλεφώνου
                if ((await _userService.PersistUserAsync(new UserPersist
                {
                    Email = email,
                    HasEmailVerified = true
                })) is string userId && !string.IsNullOrEmpty(userId)
                )
                {
                    return Ok();
                }

                // LOGS //
                _logger.LogError("Error στην επιβεβαίωση email χρήστη");
                return RequestHandlerTool.HandleInternalServerError(new Exception("Error στην επιβεβαίωση email χρήστη"), "POST");
            }
            catch (Exception e)
            {
                // LOGS //
                _logger.LogError(e, "Error trying to verify email");
                return RequestHandlerTool.HandleInternalServerError(e, "POST");
            }
        }

        [HttpPost("register/verified")]
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

        [HttpPost("send/reset-password-email")]
        public async Task<IActionResult> SendResetPasswordEmail([FromBody] string? email)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Αποστολή reset-password email για έναρξη επαναφοράς κωδικού
                await _userService.SendResetPasswordEmailAsync(email);
                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Αποτυχία αποστολής reset password email");
                return RequestHandlerTool.HandleInternalServerError(e, "POST");
            }
        }


        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] string? email, [FromBody] string? token, [FromBody] string? newPassword)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Reset the password
                if (!await _userService.ResetPasswordAsync(email, newPassword, token))
                {
                    // LOGS //
                    _logger.LogError("Αποτυχία επαναφοράς κωδικού");
                    ModelState.AddModelError("error", "Αποτυχία επαναφοράς κωδικού");
                    return BadRequest(ModelState);
                }

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