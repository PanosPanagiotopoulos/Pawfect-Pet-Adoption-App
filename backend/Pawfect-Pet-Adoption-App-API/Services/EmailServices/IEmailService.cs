using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;

namespace Pawfect_Pet_Adoption_App_API.Services.EmailServices
{
	// Interface για την υπηρεσία API διαχείρησης και αποστολής Email
	public interface IEmailService
	{
		// Κατασκευή ενώς unique token για verification με χρήση Guid
		static String GenerateRefreshToken() { return Guid.NewGuid().ToString(); }

		/// <summary>
		/// Αποστολή email σε χρήστη για επιβεβαίωση του email του.
		/// </summary>
		/// <param name="email">Η διεύθυνση email του χρήστη.</param>
		/// <param name="subject">Το θέμα του email.</param>
		/// <param name="message">Το μήνυμα του email.</param>
		/// <exception cref="InvalidOperationException">Ρίχνεται όταν αποτυγχάνει η αποστολή του email.</exception>
		Task SendEmailAsync(String email, String subject, String message);

		// EMAIL TEMPLATES BELOW //
		public static readonly Dictionary<EmailType, String> EmailTemplates = new Dictionary<EmailType, String>
{
	{
		EmailType.Verification,
		@"
    <!DOCTYPE html>
    <html lang=""el"">
    <head>
        <meta charset=""UTF-8"">
        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
        <title>Επιβεβαίωση Email</title>
    </head>
    <body style=""margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;"">
        <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: #f4f4f4;"">
            <tr>
                <td align=""center"">
                    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""max-width: 600px; background-color: #ffffff;"">
                        <!-- Header -->
                        <tr>
                            <td style=""padding: 30px 20px; text-align: center; background-color: #4CAF50; color: white; font-size: 28px;"">
                                Καλως ήρθες στο Pawfect!
                            </td>
                        </tr>
                        <!-- Body -->
                        <tr>
                            <td style=""padding: 40px 20px; font-size: 18px; line-height: 1.6; color: #333333;"">
                                Καλως ήρθες στην οικογένεια του Pawfect. Παρακαλώ επιβεβαίωσε το email σου στον σύνδεσμο παρακάτω:<br><br>
                                <table align=""center"" cellpadding=""0"" cellspacing=""0"" border=""0"">
                                    <tr>
                                        <td style=""background-color: #4CAF50; padding: 15px 30px; border-radius: 5px;"">
                                            <a href=""{0}"" style=""color: white; text-decoration: none; font-size: 20px;"">Verify Email</a>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                        <!-- Footer -->
                        <tr>
                            <td style=""padding: 20px; text-align: center; font-size: 14px; color: #999999; background-color: #f4f4f4;"">
                                Αν δεν δημιούργησες λογαριασμό, παρακαλώ αγνόησε αυτό το email.<br>
                                © 2023 Pawfect. Όλα τα δικαιώματα διατηρούνται.
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
    </body>
    </html>"
		},
		{
			EmailType.Reset_Password,
			@"
    <!DOCTYPE html>
    <html lang=""el"">
    <head>
        <meta charset=""UTF-8"">
        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
        <title>Επαναφορά Κωδικού</title>
    </head>
    <body style=""margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;"">
        <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: #f4f4f4;"">
            <tr>
                <td align=""center"">
                    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""max-width: 600px; background-color: #ffffff;"">
                        <!-- Header -->
                        <tr>
                            <td style=""padding: 30px 20px; text-align: center; background-color: #4CAF50; color: white; font-size: 28px;"">
                                Επαναφορά Κωδικού
                            </td>
                        </tr>
                        <!-- Body -->
                        <tr>
                            <td style=""padding: 40px 20px; font-size: 18px; line-height: 1.6; color: #333333;"">
                                Επαναφορά κωδικού στην υπηρεσία Pawfect. Παρακαλώ επιβεβαίωσε το email σου για την επαναφορά στον σύνδεσμο παρακάτω:<br><br>
                                <table align=""center"" cellpadding=""0"" cellspacing=""0"" border=""0"">
                                    <tr>
                                        <td style=""background-color: #4CAF50; padding: 15px 30px; border-radius: 5px;"">
                                            <a href=""{0}"" style=""color: white; text-decoration: none; font-size: 20px;"">Verify Email</a>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                        <!-- Footer -->
                        <tr>
                            <td style=""padding: 20px; text-align: center; font-size: 14px; color: #999999; background-color: #f4f4f4;"">
                                Αν δεν ζήτησες επαναφορά κωδικού, παρακαλώ αγνόησε αυτό το email.<br>
                                © 2023 Pawfect. Όλα τα δικαιώματα διατηρούνται.
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
    </body>
    </html>"
		}
	};
	}
}