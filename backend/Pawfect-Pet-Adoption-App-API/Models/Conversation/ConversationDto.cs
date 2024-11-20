using Pawfect_Pet_Adoption_App_API.Models.Animal;
using Pawfect_Pet_Adoption_App_API.Models.User;

namespace Pawfect_Pet_Adoption_App_API.Models.Conversation
{
    public class ConversationDto
    {
        public string Id { get; set; }
        public List<string> UsersIds { get; set; }
        public List<UserDto>? Users { get; set; }
        public string AnimalId { get; set; }
        public AnimalDto? Animal { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
