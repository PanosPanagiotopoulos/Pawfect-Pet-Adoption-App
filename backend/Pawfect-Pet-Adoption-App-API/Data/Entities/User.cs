namespace Pawfect_Pet_Adoption_App_API.Data.Entities
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;
    using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
    using System;

    /// <summary>
    /// Το κύριο μοντέλο ενώς χρήστη στο σύστημα
    /// </summary>
    public class User
    {

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        // Must be unique for each user
        public string Email { get; set; }

        [BsonIgnoreIfNull]
        public string? Password { get; set; } // Only needed for local credentials


        public string FullName { get; set; }


        /// <value>
        /// Ο ρόλος του χρήστη στο σύστημα.
        /// [ User, Shelter, Admin ]
        /// </value>
        public UserRole Role { get; set; }

        /// <value>
        /// Ο αριθμός τηλεφώνου του χρήστη στο σύστημα
        /// </value>
        public string Phone { get; set; }


        /// <value>
        /// Η τοποθεσία του χρήστη
        /// </value>
        public Location Location { get; set; }

        /// <value>
        /// Το id καταφυγίου με τα υπόλοιπα δεδομένα για τον συγκεκριμένο χρήστη.
        /// </value>
        /// Μόνο για χρήστες-καταφύγια
        [BsonIgnoreIfNull]
        public string? ShelterId { get; set; }


        /// <value>
        /// Ο τρόπος πρόσβασης του χρήστη. Π.χ Local άν συνδέεται με email, password ή Google άν συνδέεται με google.
        /// </value>
        /// [ Google, Local ]
        [BsonRepresentation(BsonType.String)]
        public AuthProvider AuthProvider { get; set; }

        /// <value>
        /// To id του χρήστη στην εξωτερική υπηρεσία που επέλεξε να εγγραφεί/συνδεθεί
        /// </value>
        [BsonIgnoreIfNull]
        public string? AuthProviderId { get; set; }


        /// <value>
        /// Υποδηλώνει άν τα στοιχεία του χρήστη έχουν επιβεβαιωθεί
        /// </value>
        public bool IsVerified { get; set; }

        /// <value>
        /// Υποδηλώνει άν το κινητό του χρήστη έχουν επιβεβαιωθεί
        /// </value>
        public bool HasPhoneVerified { get; set; }
        /// <value>
        /// Υποδηλώνει άν το email του χρήστη έχουν επιβεβαιωθεί
        /// </value>
        public bool HasEmailVerified { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
