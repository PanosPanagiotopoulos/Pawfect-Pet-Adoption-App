namespace Pawfect_Pet_Adoption_App_API.Services
{
    // Interface για την υπηρεσία API διαχείρησης και αποστολής Email
    public interface IEmailService
    {
        // Κατασκευή ενώς unique token για verification με χρήση Guid
        static string GenerateRefreshToken() { return Guid.NewGuid().ToString(); }

        // Αποστολή email σε χρήστη για επιβεβαίωση του email του
        Task SendEmailAsync(string email, string subject, string message);
    }
}
