using AutoMapper;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.Breed;

namespace Pawfect_Pet_Adoption_App_API.Builders
{
    public class AutoBreedBuilder : Profile
    {
        // Builders για μετατροπές object απο Entities σε Μοντέλα κάποιας χρήσης και αντίστροφα
        // Builder για Entity : Breed
        public AutoBreedBuilder()
        {
            // Mapping για το Entity : Breed σε Breed για χρήση του σε αντιγραφή αντικειμένων
            CreateMap<Breed, Breed>();

            // POST Request Dto Μοντέλα
            CreateMap<Breed, BreedPersist>();
            CreateMap<BreedPersist, Breed>();
        }
    }

    // TODO : GET Response Dto Μοντέλα
}