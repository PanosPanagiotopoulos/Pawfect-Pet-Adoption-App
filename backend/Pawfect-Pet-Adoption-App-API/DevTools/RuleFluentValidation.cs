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
        public static bool BeAValidDate(DateTime? date)
        {
            return date.HasValue && date.Value != default(DateTime);
        }

        /// <summary>
        /// Ελέγχει αν το id είναι έγκυρο ObjectId.
        /// </summary>
        /// <param name="id">Το id που θα ελεγχθεί.</param>
        /// <returns>Επιστρέφει true αν το id είναι έγκυρο ObjectId, αλλιώς false.</returns>
        public static bool IsObjectId(string? id)
        {
            if (id == null) return true;

            return ObjectId.TryParse(id, out _);
        }
    }
}
