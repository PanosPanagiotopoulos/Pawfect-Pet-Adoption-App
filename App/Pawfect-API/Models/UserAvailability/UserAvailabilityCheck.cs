using System.ComponentModel.DataAnnotations;

namespace Pawfect_Pet_Adoption_App_API.Models.UserAvailability
{
    public class UserAvailabilityCheck
    {
        [EmailAddress]
        public String? Email { get; set; }

        [Phone]
        public String? Phone { get; set; }
    }
}
