using System.ComponentModel.DataAnnotations;

namespace Pawfect_API.Models.UserAvailability
{
    public class UserAvailabilityCheck
    {
        [EmailAddress]
        public String? Email { get; set; }

        [Phone]
        public String? Phone { get; set; }
    }
}
