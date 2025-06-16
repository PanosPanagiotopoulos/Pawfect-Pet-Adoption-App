using Main_API.Data.Entities.EnumTypes;

namespace Main_API.Services.EmailServices
{
    public interface IEmailService
    {
        // Κατασκευή ενός unique token για verification με χρήση Guid
        static String GenerateRefreshToken() => Guid.NewGuid().ToString();

        /// <summary>
        /// Αποστολή email σε χρήστη για επιβεβαίωση του email του.
        /// </summary>
        /// <param name="email">Η διεύθυνση email του χρήστη.</param>
        /// <param name="subject">Το θέμα του email.</param>
        /// <param name="message">Το μήνυμα του email.</param>
        /// <exception cref="InvalidOperationException">Ρίχνεται όταν αποτυγχάνει η αποστολή του email.</exception>
        Task SendEmailAsync(String email, String subject, String message);

        /// <summary>
        /// Φορτώνει και επεξεργάζεται ένα template email από αρχείο, αντικαθιστώντας placeholders με παραμέτρους.
        /// </summary>
        /// <param name="emailType">Ο τύπος του email (π.χ. Verification, Reset_Password).</param>
        /// <param name="parameters">Παράμετροι για αντικατάσταση στο template (π.χ. UserName, VerificationLink).</param>
        /// <returns>Το επεξεργασμένο HTML περιεχόμενο του email.</returns>
        /// <exception cref="FileNotFoundException">Ρίχνεται αν το template δεν βρεθεί.</exception>
        /// <exception cref="InvalidOperationException">Ρίχνεται αν το template είναι κενό ή μη έγκυρο.</exception>
        Task<String> GetEmailTemplateAsync(EmailType emailType, Dictionary<String, String> parameters);
    }
}