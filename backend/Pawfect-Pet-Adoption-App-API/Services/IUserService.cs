using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models;
using Pawfect_Pet_Adoption_App_API.Models.User;

namespace Pawfect_Pet_Adoption_App_API.Services
{
    public interface IUserService
    {
        Task<string> RegisterUserUnverifiedAsync(RegisterPersist registerPersist);
        bool VerifyOtp(string phonenumber, OTPVerification otpVerification);
        Task GenerateNewOtp(string phonenumber);
        Task SendVerficationEmailAsync(string email);
        bool VerifyEmail(string email, string token);
        Task<string?> PersistUser(UserPersist userPersist);
        Task<string?> PersistUser(User user);
        Task<bool> VerifyUser(RegisterPersist toRegisterUser);
        Task<User?> RetrieveUser(string id, string email);


    }
}
