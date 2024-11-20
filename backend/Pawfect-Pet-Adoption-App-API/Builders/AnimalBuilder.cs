using AutoMapper;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.Animal;

namespace Pawfect_Pet_Adoption_App_API.Builders
{
    public class AnimalBuilder : Profile
    {
        // Builders για μετατροπές object απο Entities σε Μοντέλα κάποιας χρήσης και αντίστροφα
        // Builder για Entity : Animal
        public AnimalBuilder()
        {
            // Mapping για το Entity : Animal σε Animal για χρήση του σε αντιγραφή αντικειμένων
            CreateMap<Animal, Animal>();

            // GET Response Dto Μοντέλα
            CreateMap<Animal, AnimalDto>();
            CreateMap<AnimalDto, Animal>();

            // POST Request Dto Μοντέλα
            CreateMap<Animal, AnimalPersist>();
            CreateMap<AnimalPersist, Animal>();
        }
    }
}