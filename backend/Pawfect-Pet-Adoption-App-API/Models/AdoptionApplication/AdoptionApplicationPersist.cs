using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;

namespace Pawfect_Pet_Adoption_App_API.Models.AdoptionApplication
{
    public class AdoptionApplicationPersist
    {
        public String Id { get; set; }
        public String UserId { get; set; }
        public String AnimalId { get; set; }
        public String ShelterId { get; set; }
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
        public String ApplicationDetails { get; set; }
    }
}
