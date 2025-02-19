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
            if (String.IsNullOrEmpty(value))
            {
                throw new InvalidDataException("Null or Empty reference σε value προς Hashing");
            }

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(value));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        // Validation hash ενώς String με ενώς άλλου
        public static Boolean ValidatedHashedValues(String? enteredValue, String? existingValue)
        {
            Log.Information("Entered Value = " + enteredValue + " Hashed Value = " + existingValue);
            if (String.IsNullOrEmpty(enteredValue) || String.IsNullOrEmpty(existingValue))
            {
                throw new InvalidDataException("Null or Empty reference σε value προς Hash Validation");
            }

            String hashedEnteredValue = HashValue(enteredValue);
            return hashedEnteredValue == existingValue;
        }
    }
}
