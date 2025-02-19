using Pawfect_Pet_Adoption_App_API.Models.Shelter;
using Pawfect_Pet_Adoption_App_API.Models.User;

namespace Pawfect_Pet_Adoption_App_API.Models
{

    // Μοντέλο για την initial εγγραφή χρήστη ενώς χρήστη στο σύστημα στο σύστημα
    public class RegisterPersist
    {
        // Δεδομένα γενικού χρήστη
        public UserPersist User { get; set; }

        // Δεδομένα shelter στην περίπτωση που είναι. null σε περίπτωση που δεν είναι
        public ShelterPersist Shelter { get; set; }
    }
}
