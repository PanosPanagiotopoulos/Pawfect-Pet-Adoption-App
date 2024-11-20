namespace Pawfect_Pet_Adoption_App_API.Models.Message
{
    public class MessagePersist
    {
        public string Id { get; set; }

        public string ConversationId { get; set; }

        public string SenderId { get; set; }

        public string RecepientId { get; set; }

        public string Content { get; set; }

        public bool IsRead { get; set; } = false;
    }
}
