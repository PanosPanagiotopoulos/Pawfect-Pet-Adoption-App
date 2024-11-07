namespace Pawfect_Pet_Adoption_App_API.Models
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;
    using Pawfect_Pet_Adoption_App_API.Models.EnumTypes;
    using System;
    using System.ComponentModel.DataAnnotations;

    // User Model
    /// <summary>
    /// Το κύριο μοντέλο ενώς χρήστη στο σύστημα
    /// </summary>
    public class User
    {

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [Required(ErrorMessage = "Ένα πραγματικό Email είναι απαραίτητο για αυτή τη ενέργεια")]
        [EmailAddress]
        // Must be unique for each user
        public string Email { get; set; }

        [BsonIgnoreIfNull]
        [MinLength(7, ErrorMessage = "Ο κωδικός ενώς χρήστη πρέπει να έχει τουλάχιστον 6 χαρακτήρες.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+{}\[\]:;<>,.?~\\/-]).{7,}$",
        ErrorMessage = "Ο κωδικός ενώς χρήστη πρέπει να έχει τουλάχιστον 6 χαρακτήρες, τουλάχιστον 1 κεφαλαίο, έναν αριθμό και έναν ειδικό χαρακτήρα.")]
        public string? Password { get; set; } // Only needed for local credentials


        [Required(ErrorMessage = "Το ονοματεπώνυμο είναι απαραίτητο.")]
        [MinLength(5, ErrorMessage = "Το ονοματεπώνυμο δεν μπορεί να έχει λιγότερο απο 5 χαρακτήρες.")]
        public string FullName { get; set; }


        /// <value>
        /// Ο ρόλος του χρήστη στο σύστημα.
        /// </value>
        [Required(ErrorMessage = "Ο ρόλος του χρήστη είναι απαραίτητος, Παρακαλώ επιλέξτε μεταξύ: (User, Shelter, Admin).")]
        [BsonRepresentation(BsonType.String)]
        public UserRole Role { get; set; } // Enum: User, Shelter, Admin

        /// <value>
        /// Ο αριθμός τηλεφώνου του χρήστη στο σύστημα
        /// </value>
        [Required(ErrorMessage = "Ο αριθμός τηλεφώνου είναι απαραίτητος.")]
        [Phone(ErrorMessage = "Παρακαλώ εισάγετε έναν έγκυρο αριθμό τηλεφώνου.")]
        public string Phone { get; set; }


        /// <value>
        /// Η τοποθεσία του χρήστη
        /// </value>
        [Required(ErrorMessage = "Όλα τα στοιχεία τοποθεσίας σας είναι απαραίτητα.")]
        public Location Location { get; set; }

        /// <value>
        /// Το id καταφυγίου με τα υπόλοιπα δεδομένα για τον συγκεκριμένο χρήστη.
        /// </value>
        /// 
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonIgnoreIfNull]
        public string ShelterId { get; set; } = null; // Μόνο για χρήστες-καταφύγια


        /// <value>
        /// Ο τρόπος πρόσβασης του χρήστη. Π.χ Local άν συνδέεται με email, password ή Google άν συνδέεται με google.
        /// </value>
        /// 
        [BsonRepresentation(BsonType.String)]
        public AuthProvider AuthProvider { get; set; } // Enum: Google, Facebook, Local

        /// <value>
        /// To id του χρήστη στην εξωτερική υπηρεσία που επέλεξε να εγγραφεί/συνδεθεί
        /// </value>
        [BsonIgnoreIfNull]
        public string? AuthProviderId { get; set; }


        /// <value>
        /// Υποδηλώνει άν τα στοιχεία του χρήστη έχουν επιβεβαιωθεί
        /// </value>
        public bool IsVerified { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

}
