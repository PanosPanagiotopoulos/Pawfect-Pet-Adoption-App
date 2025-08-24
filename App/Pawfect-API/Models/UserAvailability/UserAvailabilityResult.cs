namespace Pawfect_Pet_Adoption_App_API.Models.UserAvailability
{
    public class UserAvailabilityResult
    {
        public Boolean IsEmailAvailable { get; set; } = true;
        public Boolean IsPhoneAvailable { get; set; } = true;
        public String? EmailMessage { get; set; }
        public String? PhoneMessage { get; set; }
    }
}
