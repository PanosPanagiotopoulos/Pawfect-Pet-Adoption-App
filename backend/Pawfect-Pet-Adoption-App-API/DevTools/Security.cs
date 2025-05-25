namespace Pawfect_Pet_Adoption_App_API.DevTools
{
	using Serilog;

	using System.Security.Cryptography;
	using System.Text;

	// Κλάση όπου θα παρέχει υπηρεσίες ασφάλειας κωδικών , domain κ.α
	public static class Security
	{
		// Υπολογισμός hash ενώς String
		public static String HashValue(String? value)
		{
			if (IsHashed(value))
			{
				Log.Information("Value : " + value + " is already hashed");
				return value;
			}

			using (SHA256 sha256 = SHA256.Create())
			{
				byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(value));
				return Convert.ToBase64String(hashedBytes);
			}
		}
		public static Boolean IsHashed(String? value)
		{
			if (String.IsNullOrEmpty(value))
				return false;

			try
			{
				byte[] data = Convert.FromBase64String(value);
				return data.Length == 32 || data.Length == 64;
			}
			catch (FormatException)
			{
				return false;
			}
		}

		// Validation hash ενώς String με ενώς άλλου
		public static Boolean ValidatedHashedValues(String enteredValue, String existingValue)
		{
			Log.Information("Entered Value = " + enteredValue + " Hashed Value = " + existingValue);
			if (String.IsNullOrEmpty(enteredValue) || String.IsNullOrEmpty(existingValue))
			{
				throw new ArgumentException("Null or Empty reference σε value προς Hash Validation");
			}

			String hashedEnteredValue = HashValue(enteredValue);
			return hashedEnteredValue == existingValue;
		}
	}
}
