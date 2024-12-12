using AutoMapper;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.User;

namespace Pawfect_Pet_Adoption_App_API.Builders
{
    public class AutoUserBuilder : Profile
    {
        // Builders για μετατροπές object απο Entities σε Μοντέλα κάποιας χρήσης και αντίστροφα
        // Builder για Entity : User
        public AutoUserBuilder()
        {
            // Mapping για nested object : Location
            CreateMap<Location, Location>();

            // Mapping για το Entity : User σε User για χρήση του σε αντιγραφή αντικειμένων
            CreateMap<User, User>();

            // POST Request Dto Μοντέλα
            CreateMap<User, UserPersist>();
            CreateMap<UserPersist, User>();
        }
    }

    // TODO: GET Response Dto Μοντέλα
}
