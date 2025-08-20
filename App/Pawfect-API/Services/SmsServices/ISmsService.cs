using Pawfect_API.Data.Entities.EnumTypes;

using System.Text.RegularExpressions;

namespace Pawfect_API.Services.SmsServices
{
	public interface ISmsService
	{
		private static readonly Dictionary<String, int> CountryCodeLengths = new Dictionary<String, int>
		{
			{ "30", 10 }, // Greece
            { "31", 9 },  // Netherlands
            { "32", 9 },  // Belgium
            { "33", 9 },  // France
            { "34", 9 },  // Spain
            { "350", 8 }, // Gibraltar
            { "351", 9 }, // Portugal
            { "352", 9 }, // Luxembourg
            { "353", 9 }, // Ireland
            { "354", 7 }, // Iceland
            { "355", 9 }, // Albania
            { "356", 8 }, // Malta
            { "357", 8 }, // Cyprus
            { "358", 9 }, // Finland
            { "359", 9 }, // Bulgaria
            { "36", 9 },  // Hungary
            { "370", 8 }, // Lithuania
            { "371", 8 }, // Latvia
            { "372", 7 }, // Estonia
            { "373", 8 }, // Moldova
            { "374", 8 }, // Armenia
            { "375", 9 }, // Belarus
            { "376", 6 }, // Andorra
            { "377", 8 }, // Monaco
            { "378", 10 }, // San Marino
            { "380", 9 }, // Ukraine
            { "381", 9 }, // Serbia
            { "382", 8 }, // Montenegro
            { "385", 9 }, // Croatia
            { "386", 8 }, // Slovenia
            { "387", 8 }, // Bosnia and Herzegovina
            { "389", 8 }, // North Macedonia
            { "39", 10 }, // Italy
            { "40", 10 }, // Romania
            { "41", 9 },  // Switzerland
            { "420", 9 }, // Czech Republic
        };
		// SMS templates
		public static readonly Dictionary<SmsType, String> SmsTemplates = new Dictionary<SmsType, String>
		{
			{ SmsType.OTP, "Your one time password is : {0}" }
		};



		/// <summary>
		/// Αποστολή SMS σε χρήστη.
		/// </summary>
		/// <param name="phoneNumber">Ο αριθμός τηλεφώνου του χρήστη.</param>
		/// <param name="message">Το μήνυμα του SMS.</param>
		/// <exception cref="InvalidOperationException">Ρίχνεται όταν αποτυγχάνει η αποστολή του SMS.</exception>
		/// <exception cref="Exception">Ρίχνεται όταν αποτυγχάνει η αποστολή του SMS με συγκεκριμένο status code και response.</exception>
		Task SendSmsAsync(String phoneNumber, String message);

		static int GenerateOtp() { return new Random().Next(100000, 999999); }


        // Αναλύει έναν αριθμό τηλεφώνου για να κρατήσει μόνο τα ψηφία του, συμπεριλαμβανομένου του κωδικού χώρας στην αρχή.
        // Επικυρώνει το μήκος του αριθμού τηλεφώνου με βάση τον δοθέντα κωδικό χώρας.

        // <param name="phoneNumber">Ο αριθμός τηλεφώνου σε οποιαδήποτε μορφή.</param>
        // <returns>Ο καθαρισμένος αριθμός τηλεφώνου μόνο με αριθμούς, αν είναι έγκυρος.</returns>
        // <exception cref="ArgumentException">Προκαλείται όταν ο αριθμός τηλεφώνου είναι άκυρος ή κενός.</exception>
        // <exception cref="FormatException">Προκαλείται όταν η μορφή του αριθμού τηλεφώνου είναι άκυρη.</exception>
        static String ParsePhoneNumber(String phoneNumber)
        {
            if (String.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("Ο αριθμός τηλεφώνου είναι null ή κενός.", nameof(phoneNumber));

            // keep only digits
            String cleanedNumber = Regex.Replace(phoneNumber, @"\D", "");

            foreach (String countryCode in CountryCodeLengths.Keys)
            {
                if (cleanedNumber.StartsWith(countryCode))
                {
                    int expectedLength = CountryCodeLengths[countryCode] + countryCode.Length;

                    if (cleanedNumber.Length == expectedLength)
                    {
                        return $"+{cleanedNumber}";
                    }

                    throw new FormatException(
                        $"Ο αριθμός τηλεφώνου δεν είναι σωστός για κωδικό χώρας +{countryCode}. " +
                        $"Αναμενόμενο μήκος: {expectedLength - countryCode.Length}, " +
                        $"Δοσμένο μήκος: {cleanedNumber.Length - countryCode.Length}");
                }
            }

            throw new FormatException("Ο αριθμός τηλεφώνου δεν περιέχει έγκυρο διεθνή κωδικό χώρας.");
        }
    }
}
