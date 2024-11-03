using System.ComponentModel.DataAnnotations;

namespace Pawfect_Pet_Adoption_App_API.Models
{
    /// <summary>
    ///   Τα links των social media για το καταφύγιο.
    /// </summary>
    public class SocialMedia
    {
        [RegularExpression(@"^https?:\/\/(www\.)?facebook\.com\/[A-Za-z0-9._%-]+$", ErrorMessage = "Παρακαλώ εισάγετε έγκυρο link Facebook")]
        public string Facebook { get; set; }

        [RegularExpression(@"^https?:\/\/(www\.)?instagram\.com\/[A-Za-z0-9._%-]+$", ErrorMessage = "Παρακαλώ εισάγετε έγκυρο link Instagram")]
        public string Instagram { get; set; }
    }
}
