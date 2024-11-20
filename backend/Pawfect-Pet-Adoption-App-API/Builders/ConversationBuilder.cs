using AutoMapper;
using Pawfect_Pet_Adoption_App_API.Models.Conversation;

namespace Pawfect_Pet_Adoption_App_API.Builders
{
    public class ConversationBuilder : Profile
    {
        // Builders για μετατροπές object απο Entities σε Μοντέλα κάποιας χρήσης και αντίστροφα
        // Builder για Entity : Conversation
        public ConversationBuilder()
        {
            // Mapping για το Entity : Conversation σε Conversation για χρήση του σε αντιγραφή αντικειμένων
            CreateMap<Conversation, Conversation>();

            // GET Response Dto Μοντέλα
            CreateMap<Conversation, ConversationDto>();
            CreateMap<ConversationDto, Conversation>();

            // POST Request Dto Μοντέλα
            CreateMap<Conversation, ConversationPersist>();
            CreateMap<ConversationPersist, Conversation>();
        }
    }
}