using Pawfect_Notifications.Data.Entities;
using Pawfect_Notifications.Repositories.Interfaces;
using Pawfect_Notifications.Services.MongoServices;

namespace Pawfect_Notifications.Repositories.Implementations
{
	public class NotificationRepository : BaseMongoRepository<Notification>, INotificationRepository
	{
		public NotificationRepository(MongoDbService dbService) : base(dbService) { }
	}
}
