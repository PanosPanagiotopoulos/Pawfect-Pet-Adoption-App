using AutoMapper;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.Message;

namespace Pawfect_Pet_Adoption_App_API.Builders
{
    public class AutoMessageBuilder : Profile
    {
        // Builders για μετατροπές object απο Entities σε Μοντέλα κάποιας χρήσης και αντίστροφα
        // Builder για Entity : Message
        public AutoMessageBuilder()
        {
            // Mapping για το Entity : Message σε Message για χρήση του σε αντιγραφή αντικειμένων
            CreateMap<Message, Message>();


            // POST Request Dto Μοντέλα
            CreateMap<Message, MessagePersist>();
            CreateMap<MessagePersist, Message>();
        }

        // TODO : GET Response Dto Μοντέλα
    }
}