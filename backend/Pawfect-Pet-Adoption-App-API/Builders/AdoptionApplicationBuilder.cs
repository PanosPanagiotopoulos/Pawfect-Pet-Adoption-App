using AutoMapper;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.AdoptionApplication;

namespace Pawfect_Pet_Adoption_App_API.Builders
{
    public class AdoptionApplicationBuilder : Profile
    {
        // Builders για μετατροπές object απο Entities σε Μοντέλα κάποιας χρήσης και αντίστροφα
        // Builder για Entity : AdoptionApplication
        public AdoptionApplicationBuilder()
        {
            // Mapping για το Entity : AdoptionApplication σε AdoptionApplication για χρήση του σε αντιγραφή αντικειμένων
            CreateMap<AdoptionApplication, AdoptionApplication>();

            // GET Response Dto Μοντέλα
            CreateMap<AdoptionApplication, AdoptionApplicationDto>();
            CreateMap<AdoptionApplicationDto, AdoptionApplication>();

            // POST Request Dto Μοντέλα
            CreateMap<AdoptionApplication, AdoptionApplicationPersist>();
            CreateMap<AdoptionApplicationPersist, AdoptionApplication>();
        }
    }
}