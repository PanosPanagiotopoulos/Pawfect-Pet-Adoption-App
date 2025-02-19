namespace Pawfect_Pet_Adoption_App_API.Models.Message
{
    public class MessagePersist
    {
        public String Id { get; set; }

        public String ConversationId { get; set; }

        public String SenderId { get; set; }

        public String RecipientId { get; set; }

        public String Content { get; set; }

        public Boolean IsRead { get; set; } = false;
    }
}
