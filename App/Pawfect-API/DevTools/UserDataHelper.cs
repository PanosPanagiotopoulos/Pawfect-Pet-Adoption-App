namespace Pawfect_API.DevTools
{
    public static class UserDataHelper
    {
        public static String GetFirstNameFormatted(String fullName)
        {
            if (String.IsNullOrWhiteSpace(fullName))
                return String.Empty;

            // Split the full name and get the first part
            String[] nameParts = fullName.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (nameParts.Length == 0)
                return String.Empty;

            String firstName = nameParts[0];

            if (firstName.Length > 1 &&
                 firstName.EndsWith("ς", StringComparison.OrdinalIgnoreCase) ||
                 firstName.EndsWith("σ", StringComparison.OrdinalIgnoreCase))
            {
                firstName = firstName.Substring(0, firstName.Length - 1);
            }

            return firstName;
        }
    }
}
