using AutoMapper;
using Pawfect_Pet_Adoption_App_API.Data.Entities.HelperModels;
using Pawfect_Pet_Adoption_App_API.Models.Shelter;

namespace Pawfect_Pet_Adoption_App_API.Builders
{
    public class AutoShelterBuilder : Profile
    {
        // Builders για μετατροπές object απο Entities σε Μοντέλα κάποιας χρήσης και αντίστροφα
        // Builder για Entity : Shelter
        public AutoShelterBuilder()
        {
            // Mapping για nested object : OpeningHours
            CreateMap<OperatingHours, OperatingHours>();
            // Mapping για nested object : SocialMedia
            CreateMap<SocialMedia, SocialMedia>();

            // Mapping για το Entity : Shelter σε Shelter για χρήση του σε αντιγραφή αντικειμένων
            CreateMap<Shelter, Shelter>();

            // POST Request Dto Μοντέλα
            CreateMap<Shelter, ShelterPersist>();
            CreateMap<ShelterPersist, Shelter>();
        }
    }

    // TODO: GET Response Dto Μοντέλα

}