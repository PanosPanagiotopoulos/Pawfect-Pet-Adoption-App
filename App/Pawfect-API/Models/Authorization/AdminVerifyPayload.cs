using System.ComponentModel.DataAnnotations;

namespace Pawfect_Pet_Adoption_App_API.Models.Authorization
{
    public class AdminVerifyPayload
    {
        [Required]
        public String AdminToken { get; set; }
        [Required]
        public Boolean Accept { get; set; }
    }
}
