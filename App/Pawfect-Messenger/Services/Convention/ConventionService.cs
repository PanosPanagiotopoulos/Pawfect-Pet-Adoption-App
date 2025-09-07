using Pawfect_Messenger.DevTools;

namespace Pawfect_Messenger.Services.Convention
{
    public class ConventionService : IConventionService
    {
        /// <summary>
        /// Checks if a String is a valid MongoDB ObjectId.
        /// </summary>
        /// <param name="id">The String to validate.</param>
        /// <returns>True if the String is a valid ObjectId, otherwise false.</returns>
        public Boolean IsValidId(String id) => !String.IsNullOrEmpty(id) && RuleFluentValidation.IsObjectId(id);
    }
}