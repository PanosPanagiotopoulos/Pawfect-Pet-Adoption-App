using Pawfect_Pet_Adoption_App_API.Models.UserAvailability;

namespace Pawfect_Pet_Adoption_App_API.Services.UserServices
{
    public interface IUserAvailabilityService
    {
        Task<UserAvailabilityResult> CheckUserAvailabilityAsync(UserAvailabilityCheck availabilityCheck);
        void InvalidateAvailabilityCache(String email, String phone);
    }
}
