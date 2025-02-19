using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;

namespace Pawfect_Pet_Adoption_App_API.Models
{
    public class LoginPayload
    {
        public String? Email { get; set; }
        public String? Password { get; set; }
        public String? ProviderAccessCode { get; set; }
        public AuthProvider LoginProvider { get; set; }
    }
}
