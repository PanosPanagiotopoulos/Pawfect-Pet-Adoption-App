using AutoMapper;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.Breed;

namespace Pawfect_Pet_Adoption_App_API.Builders
{
    public class BreedBuilder : Profile
    {
        // Builders για μετατροπές object απο Entities σε Μοντέλα κάποιας χρήσης και αντίστροφα
        // Builder για Entity : Breed
        public BreedBuilder()
        {
            // Mapping για το Entity : Breed σε Breed για χρήση του σε αντιγραφή αντικειμένων
            CreateMap<Breed, Breed>();

            // GET Response Dto Μοντέλα
            CreateMap<Breed, BreedDto>();
            CreateMap<BreedDto, Breed>();

            // POST Request Dto Μοντέλα
            CreateMap<Breed, BreedPersist>();
            CreateMap<BreedPersist, Breed>();
        }
    }
}