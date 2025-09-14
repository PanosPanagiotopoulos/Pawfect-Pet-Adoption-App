using Pawfect_Messenger.Models.User;

namespace Pawfect_Messenger.Hubs.ChatHub
{
    public interface IChatClient
    {
        Task MessageReceived(Models.Message.Message message);
        Task MessageStatusChanged(Models.Message.Message message);
        Task MessageRead(Models.Message.Message message);
        Task LastConversationMessageUpdated(Models.Message.Message message);

        Task PresenceChanged(UserPresence userPresence);
    }
}
