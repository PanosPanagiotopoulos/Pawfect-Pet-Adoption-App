using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;

namespace Pawfect_Pet_Adoption_App_API.Models.AdoptionApplication
{
    public class AdoptionApplicationPersist
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string AnimalId { get; set; }
        public string ShelterId { get; set; }
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
        public string ApplicationDetails { get; set; }
    }
}
