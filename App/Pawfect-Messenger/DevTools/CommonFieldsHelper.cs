namespace Pawfect_Messenger.DevTools
{
    public static class CommonFieldsHelper
    {
        public static List<String> ConversationFields()
        {
            return [
                nameof(Models.Conversation.Conversation.Id),
                nameof(Models.Conversation.Conversation.CreatedAt),
                nameof(Models.Conversation.Conversation.LastMessageAt),

                String.Join('.', nameof(Models.Conversation.Conversation.LastMessagePreview), nameof(Models.Message.Message.Content)),
                String.Join('.', nameof(Models.Conversation.Conversation.LastMessagePreview), nameof(Models.Message.Message.Status)),
                String.Join('.', nameof(Models.Conversation.Conversation.LastMessagePreview), nameof(Models.Message.Message.UpdatedAt)),

                String.Join('.', nameof(Models.Conversation.Conversation.LastMessagePreview), nameof(Models.Message.Message.Sender), nameof(Models.User.User.FullName)),
                String.Join('.', nameof(Models.Conversation.Conversation.LastMessagePreview), nameof(Models.Message.Message.Sender), nameof(Models.User.User.ProfilePhoto), nameof(Models.File.File.SourceUrl)),
                String.Join('.', nameof(Models.Conversation.Conversation.LastMessagePreview), nameof(Models.Message.Message.Sender), nameof(Models.User.User.Shelter), nameof(Models.Shelter.Shelter.ShelterName)),

                String.Join('.', nameof(Models.Conversation.Conversation.CreatedBy), nameof(Models.User.User.FullName)),
                String.Join('.', nameof(Models.Conversation.Conversation.CreatedBy), nameof(Models.User.User.Shelter), nameof(Models.Shelter.Shelter.ShelterName)),
                String.Join('.', nameof(Models.Conversation.Conversation.CreatedBy), nameof(Models.User.User.ProfilePhoto), nameof(Models.File.File.SourceUrl)),


                String.Join('.', nameof(Models.Conversation.Conversation.Participants), nameof(Models.User.User.FullName)),
                String.Join('.', nameof(Models.Conversation.Conversation.Participants), nameof(Models.User.User.Shelter), nameof(Models.Shelter.Shelter.ShelterName)),
                String.Join('.', nameof(Models.Conversation.Conversation.Participants), nameof(Models.User.User.ProfilePhoto), nameof(Models.File.File.SourceUrl)),
            ];
        }

        public static List<string> MessageFields()
        {
            return [
                // Base message fields
                nameof(Models.Message.Message.Id),
                nameof(Models.Message.Message.Content),
                nameof(Models.Message.Message.Type),
                nameof(Models.Message.Message.Status),
                nameof(Models.Message.Message.CreatedAt),
                nameof(Models.Message.Message.UpdatedAt),

                // Conversation (only Id for lightweight joins)
                String.Join('.', nameof(Models.Message.Message.Conversation), nameof(Models.Conversation.Conversation.Id)),

                // Sender (basic profile preview)
                String.Join('.', nameof(Models.Message.Message.Sender), nameof(Models.User.User.FullName)),
                String.Join('.', nameof(Models.Message.Message.Sender), nameof(Models.User.User.Shelter), nameof(Models.Shelter.Shelter.ShelterName)),
                String.Join('.', nameof(Models.Message.Message.Sender), nameof(Models.User.User.ProfilePhoto), nameof(Models.File.File.SourceUrl)),

                // ReadBy (list of users with minimal preview)
                String.Join('.', nameof(Models.Message.Message.ReadBy), nameof(Models.User.User.FullName)),
                String.Join('.', nameof(Models.Message.Message.ReadBy), nameof(Models.User.User.Shelter), nameof(Models.Shelter.Shelter.ShelterName)),
                String.Join('.', nameof(Models.Message.Message.ReadBy), nameof(Models.User.User.ProfilePhoto), nameof(Models.File.File.SourceUrl)),
            ];
        }
    }
}
