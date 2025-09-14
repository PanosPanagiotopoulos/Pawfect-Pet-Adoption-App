using Pawfect_Messenger.Models.User;

namespace Pawfect_Messenger.Services.PresenceServices
{
    public interface IPresenceService
    {
        Task MarkOnline(String userId, String connectionId);
        Task MarkOffline(String userId, String connectionId);
        Boolean IsOnline(String userId);
        Task<UserPresence> GetPresence(String userId);
    }
}
