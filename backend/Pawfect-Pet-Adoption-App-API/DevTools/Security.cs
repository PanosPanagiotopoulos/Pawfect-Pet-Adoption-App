namespace Pawfect_Pet_Adoption_App_API.DevTools
{
    using System.Security.Cryptography;
    using System.Text;

    // Κλάση όπου θα παρέχει υπηρεσίες ασφάλειας κωδικών , domain κ.α
    public static class Security
    {
        // Υπολογισμός hash ενώς string
        public static string HashValue(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new InvalidDataException("Null or Empty reference σε value προς Hashing");
            }

            using (SHA256 sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(value));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        // Validation hash ενώς string με ενώς άλλου
        public static bool ValidatedHashedValues(string? enteredValue, string? hashedValue)
        {
            if (string.IsNullOrEmpty(enteredValue) || string.IsNullOrEmpty(hashedValue))
            {
                throw new InvalidDataException("Null or Empty reference σε value προς Hash Validation");
            }

            string hashedEnteredValue = HashValue(enteredValue);
            return hashedEnteredValue == hashedValue;
        }
    }
}
