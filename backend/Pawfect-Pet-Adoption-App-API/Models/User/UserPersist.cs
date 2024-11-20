using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;

namespace Pawfect_Pet_Adoption_App_API.Models.User
{
    public class UserPersist
    {
        public string Id { get; set; }

        public string Email { get; set; }

        public string? Password { get; set; }


        public string FullName { get; set; }


        /// <value>
        /// Ο ρόλος του χρήστη στο σύστημα.
        /// </value>
        public UserRole Role { get; set; } // Enum: User, Shelter, Admin

        /// <value>
        /// Ο αριθμός τηλεφώνου του χρήστη στο σύστημα
        /// </value>
        public string Phone { get; set; }


        /// <value>
        /// Η τοποθεσία του χρήστη
        /// </value>
        public Location Location { get; set; }


        /// <value>
        /// Ο τρόπος πρόσβασης του χρήστη. Π.χ Local άν συνδέεται με email, password ή Google άν συνδέεται με google.
        /// </value>
        /// [ Google, Local ]
        public AuthProvider AuthProvider { get; set; }

        /// <value>
        /// To id του χρήστη στην εξωτερική υπηρεσία που επέλεξε να εγγραφεί/συνδεθεί
        /// </value>
        public string? AuthProviderId { get; set; }

    }
}
