using Pawfect_Pet_Adoption_App_API.Models.EnumTypes;

namespace Pawfect_Pet_Adoption_App_API.DTOs.User
{
    public class GUserDTO
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public UserRole Role { get; set; }
        public string Phone { get; set; }
        public Location Location { get; set; }
        public string ShelterId { get; set; }
        public AuthProvider AuthProvider { get; set; }
        public string AuthProviderId { get; set; }
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
