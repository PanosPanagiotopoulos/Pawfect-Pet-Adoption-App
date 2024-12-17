using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
using Pawfect_Pet_Adoption_App_API.Models.Shelter;

namespace Pawfect_Pet_Adoption_App_API.Models.User
{
    public class UserDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public UserRole Role { get; set; }
        public string Phone { get; set; }
        public Location Location { get; set; }
        public ShelterDto? Shelter { get; set; }
        public AuthProvider AuthProvider { get; set; }
        public string? AuthProviderId { get; set; }
        public bool IsVerified { get; set; }
        public bool HasPhoneVerified { get; set; }
        public bool HasEmailVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
