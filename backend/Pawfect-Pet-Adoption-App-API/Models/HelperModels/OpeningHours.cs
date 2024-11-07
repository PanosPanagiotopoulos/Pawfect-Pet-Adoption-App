using System.ComponentModel.DataAnnotations;

namespace Pawfect_Pet_Adoption_App_API.Models.HelperModels
{
    /// <summary>
    ///   Οι ώρες λειτουργίας του καταφυγίου
    /// </summary>
    public class OperatingHours
    {
        [Required(ErrorMessage = "Η Δευτέρα απαιτείται")]
        [RegularExpression(@"^(?:[01]\d|2[0-3]):[0-5]\d,(?:[01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "Η μορφή ώρας πρέπει να είναι ΩΩ:ΛΛ,ΩΩ:ΛΛ")]
        public string Monday { get; set; }

        [Required(ErrorMessage = "Η Τρίτη απαιτείται")]
        [RegularExpression(@"^(?:[01]\d|2[0-3]):[0-5]\d,(?:[01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "Η μορφή ώρας πρέπει να είναι ΩΩ:ΛΛ,ΩΩ:ΛΛ")]
        public string Tuesday { get; set; }

        [Required(ErrorMessage = "Η Τετάρτη απαιτείται")]
        [RegularExpression(@"^(?:[01]\d|2[0-3]):[0-5]\d,(?:[01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "Η μορφή ώρας πρέπει να είναι ΩΩ:ΛΛ,ΩΩ:ΛΛ")]
        public string Wednesday { get; set; }

        [Required(ErrorMessage = "Η Πέμπτη απαιτείται")]
        [RegularExpression(@"^(?:[01]\d|2[0-3]):[0-5]\d,(?:[01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "Η μορφή ώρας πρέπει να είναι ΩΩ:ΛΛ,ΩΩ:ΛΛ")]
        public string Thursday { get; set; }

        [Required(ErrorMessage = "Η Παρασκευή απαιτείται")]
        [RegularExpression(@"^(?:[01]\d|2[0-3]):[0-5]\d,(?:[01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "Η μορφή ώρας πρέπει να είναι ΩΩ:ΛΛ,ΩΩ:ΛΛ")]
        public string Friday { get; set; }

        [Required(ErrorMessage = "Το Σάββατο απαιτείται")]
        [RegularExpression(@"^(?:[01]\d|2[0-3]):[0-5]\d,(?:[01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "Η μορφή ώρας πρέπει να είναι ΩΩ:ΛΛ,ΩΩ:ΛΛ")]
        public string Saturday { get; set; }

        [Required(ErrorMessage = "Η Κυριακή απαιτείται")]
        [RegularExpression(@"^(?:[01]\d|2[0-3]):[0-5]\d,(?:[01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "Η μορφή ώρας πρέπει να είναι ΩΩ:ΛΛ,ΩΩ:ΛΛ")]
        public string Sunday { get; set; }
    }
}
