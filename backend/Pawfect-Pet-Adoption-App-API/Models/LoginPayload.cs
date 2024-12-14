using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;

namespace Pawfect_Pet_Adoption_App_API.Models
{
    public class LoginPayload
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? ProviderAccessCode { get; set; }
        public AuthProvider LoginProvider { get; set; }
    }
}
