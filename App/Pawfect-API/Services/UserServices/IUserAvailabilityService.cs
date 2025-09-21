using Pawfect_API.Models.UserAvailability;

namespace Pawfect_API.Services.UserServices
{
    public interface IUserAvailabilityService
    {
        Task<UserAvailabilityResult> CheckUserAvailabilityAsync(UserAvailabilityCheck availabilityCheck);
        void InvalidateAvailabilityCache(String email, String phone);
    }
}
