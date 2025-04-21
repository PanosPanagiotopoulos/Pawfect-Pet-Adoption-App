using MongoDB.Bson;

namespace Pawfect_Pet_Adoption_App_API.DevTools
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
            if (id == null) return true;

            return ObjectId.TryParse(id, out _);
        }

		/// <summary>
		/// Validates if a string is a valid URL.
		/// </summary>
		/// <param name="url">The URL string to validate.</param>
		/// <returns>True if the string is a valid URL, otherwise false.</returns>
		public static Boolean IsUrl(string? url)
		{
			if (string.IsNullOrWhiteSpace(url)) return false;

			string urlPattern = @"^(https?:\/\/)?([a-zA-Z0-9\-]+\.)+[a-zA-Z]{2,}(:\d+)?(\/.*)?$";
			return System.Text.RegularExpressions.Regex.IsMatch(url, urlPattern);
		}
	}
}
