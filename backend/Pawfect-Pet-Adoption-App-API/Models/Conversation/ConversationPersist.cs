namespace Pawfect_Pet_Adoption_App_API.Models.Conversation
{
    public class ConversationPersist
    {
        public string Id { get; set; }

        public List<string> UserIds { get; set; }

        public string AnimalId { get; set; }
    }
}
