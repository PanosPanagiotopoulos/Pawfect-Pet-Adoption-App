using Pawfect_Notifications.Data.Entities;

namespace Pawfect_Notifications.Repositories.Interfaces
{
    /// <summary>
    /// Repository όπου διατηρούμε τις μη κοινές λειτουργίες
    /// Για το Collection: Notification
    /// </summary>
    public interface INotificationRepository : IMongoRepository<Notification>
    {
    }
}
