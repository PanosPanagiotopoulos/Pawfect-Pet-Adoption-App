using MongoDB.Bson;

namespace Pawfect_API.DevTools
{
    public static class RuleFluentValidation
    {
        /// <summary>
        /// Ελέγχει αν η ημερομηνία είναι έγκυρη.
        /// </summary>
        /// <param name="date">Η ημερομηνία που θα ελεγχθεί.</param>
        /// <returns>Επιστρέφει true αν η ημερομηνία είναι έγκυρη, αλλιώς false.</returns>
        public static Boolean BeAValidDate(DateTime? date)
        {
            return date.HasValue && date.Value != default(DateTime);
        }

        /// <summary>
        /// Ελέγχει αν το id είναι έγκυρο ObjectId.
        /// </summary>
        /// <param name="id">Το id που θα ελεγχθεί.</param>
        /// <returns>Επιστρέφει true αν το id είναι έγκυρο ObjectId, αλλιώς false.</returns>
        public static Boolean IsObjectId(String? id)
        {
            if (id == null) return false;

            return ObjectId.TryParse(id, out _);
        }

		/// <summary>
		/// Validates if a String is a valid URL.
		/// </summary>
		/// <param name="url">The URL String to validate.</param>
		/// <returns>True if the String is a valid URL, otherwise false.</returns>
		public static Boolean IsUrl(String? url)
		{
			if (String.IsNullOrWhiteSpace(url)) return false;

			String urlPattern = @"^(https?:\/\/)?([a-zA-Z0-9\-]+\.)+[a-zA-Z]{2,}(:\Double+)?(\/.*)?$";
			return System.Text.RegularExpressions.Regex.IsMatch(url, urlPattern);
		}
	}
}
