using AutoMapper;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.Animal;

namespace Pawfect_Pet_Adoption_App_API.Builders
{
    public class AutoAnimalBuilder : Profile
    {
        // Builders για μετατροπές object απο Entities σε Μοντέλα κάποιας χρήσης και αντίστροφα
        // Builder για Entity : Animal
        public AutoAnimalBuilder()
        {
            // Mapping για το Entity : Animal σε Animal για χρήση του σε αντιγραφή αντικειμένων
            CreateMap<Animal, Animal>();

            // POST Request Dto Μοντέλα
            CreateMap<Animal, AnimalPersist>();
            CreateMap<AnimalPersist, Animal>();
        }

        // TODO: GET Response Dto Μοντέλα

    }
}