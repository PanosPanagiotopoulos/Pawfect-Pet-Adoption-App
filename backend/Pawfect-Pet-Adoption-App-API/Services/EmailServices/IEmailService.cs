using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;

namespace Pawfect_Pet_Adoption_App_API.Services.EmailServices
{
	// Interface για την υπηρεσία API διαχείρησης και αποστολής Email
	public interface IEmailService
	{
		// Email templates
		public static readonly Dictionary<EmailType, String> EmailTemplates = new Dictionary<EmailType, String>
		{
			{ EmailType.Verification, "Καλως ήρθες στην οικογένεια του Pawfect. Παρακαλώ επιβεβαίωσε το email σου στον σύνδεσμο παρακάτω:<br><br><a href=\"{0}\" style=\"background-color: #4CAF50; color: white; padding: 10px 20px; text-align: center; text-decoration: none; display: inline-block;\">Verify Email</a>" },
			{ EmailType.Reset_Password, "Επαναφορά κωδικού στην υπηρεσία Pawfect. Παρακαλώ επιβεβαίωσε το email σου για την επαναφορά στον σύνδεσμο παρακάτω:<br><br><a href=\"{0}\" style=\"background-color: #4CAF50; color: white; padding: 10px 20px; text-align: center; text-decoration: none; display: inline-block;\">Verify Email</a>" }
		};

		// Κατασκευή ενώς unique token για verification με χρήση Guid
		static String GenerateRefreshToken() { return Guid.NewGuid().ToString(); }

		/// <summary>
		/// Αποστολή email σε χρήστη για επιβεβαίωση του email του.
		/// </summary>
		/// <param name="email">Η διεύθυνση email του χρήστη.</param>
		/// <param name="subject">Το θέμα του email.</param>
		/// <param name="message">Το μήνυμα του email.</param>
		/// <exception cref="InvalidDataException">Ρίχνεται όταν οι παράμετροι για το configuration δεν βρέθηκαν.</exception>
		/// <exception cref="InvalidOperationException">Ρίχνεται όταν αποτυγχάνει η αποστολή του email.</exception>
		Task SendEmailAsync(String email, String subject, String message);
	}
}