using AutoMapper;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.AnimalType;

namespace Pawfect_Pet_Adoption_App_API.Builders
{
    public class AnimalTypeBuilder : Profile
    {
        // Builders για μετατροπές object απο Entities σε Μοντέλα κάποιας χρήσης και αντίστροφα
        // Builder για Entity : AnimalType
        public AnimalTypeBuilder()
        {
            // Mapping για το Entity : AnimalType σε AnimalType για χρήση του σε αντιγραφή αντικειμένων
            CreateMap<AnimalType, AnimalType>();

            // GET Response Dto Μοντέλα
            CreateMap<AnimalType, AnimalTypeDto>();
            CreateMap<AnimalTypeDto, AnimalType>();

            // POST Request Dto Μοντέλα
            CreateMap<AnimalType, AnimalTypePersist>();
            CreateMap<AnimalTypePersist, AnimalType>();
        }
    }
}