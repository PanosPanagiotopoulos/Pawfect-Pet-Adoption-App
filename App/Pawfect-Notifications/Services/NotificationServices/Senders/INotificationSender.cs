using MongoDB.Driver;
using Pawfect_Notifications.Data.Entities;

namespace Pawfect_Notifications.Services.NotificationServices.Senders
{
    public interface INotificationSender
    {
        Task<Boolean> SendAsync(Notification notification, IServiceScope serviceScope, IClientSession session);
    }
}
