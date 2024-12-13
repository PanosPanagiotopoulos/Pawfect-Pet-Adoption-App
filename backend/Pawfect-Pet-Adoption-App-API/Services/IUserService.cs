using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models;
using Pawfect_Pet_Adoption_App_API.Models.User;

namespace Pawfect_Pet_Adoption_App_API.Services
{
    public interface IUserService
    {
        Task<string> RegisterUserUnverifiedAsync(RegisterPersist registerPersist);
        bool VerifyOtp(string? phonenumber, OTPVerification otpVerification);
        Task GenerateNewOtpAsync(string? phonenumber);
        Task SendVerficationEmailAsync(string? email);
        bool VerifyEmail(string? email, string? token);
        Task<string?> PersistUserAsync(UserPersist userPersist, bool allowCreation = true);
        Task<string?> PersistUserAsync(User user, bool allowCreation = true);
        Task<bool> VerifyUserAsync(RegisterPersist toRegisterUser);
        Task<User?> RetrieveUserAsync(string? id, string? email);
        Task SendResetPasswordEmailAsync(string? email);
        Task<bool> ResetPasswordAsync(string? email, string? password, string? token);
    }
}
