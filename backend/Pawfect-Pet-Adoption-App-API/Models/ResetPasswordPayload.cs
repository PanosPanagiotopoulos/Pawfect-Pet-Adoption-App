namespace Pawfect_Pet_Adoption_App_API.Models
{
    public class ResetPasswordPayload
    {
        // Τα δεδομένα Id, Email αποστέλονται ένα απο τα 2 για επιβεαβίωση του χρήστη που έκανε verify
        public string? Id { get; set; }
        public string? Email { get; set; }
        public string? Token { get; set; }
        public string? NewPassword { get; set; }
    }
}
