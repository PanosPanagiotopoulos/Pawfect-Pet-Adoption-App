using System.ComponentModel.DataAnnotations;

namespace Pawfect_API.Models.Authorization
{
    public class AdminVerifyPayload
    {
        [Required]
        public String AdminToken { get; set; }
        [Required]
        public Boolean Accept { get; set; }
    }
}
