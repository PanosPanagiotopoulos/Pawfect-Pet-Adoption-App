using Pawfect_Notifications.Data.Entities.EnumTypes;

namespace Pawfect_Notifications.Services.NotificationServices.Senders
{
    public class NotificationSenderFactory
    {
        public class NotificationSenderFactoryConfig
        {
            // Main mapper from diagram node types to their provider objects implemented. 
            // For config used with Abstract Type object 
            public Dictionary<NotificationType, Type> Accessors { get; } = new Dictionary<NotificationType, Type>();
            public NotificationSenderFactoryConfig Add(NotificationType key, Type type)
            {
                this.Accessors[key] = type;
                return this;
            }
        }
        private readonly IServiceProvider _serviceProvider;
        private Dictionary<NotificationType, Func<INotificationSender>> _accessorsMap = null;
        public NotificationSenderFactory(IServiceProvider serviceProvider, Microsoft.Extensions.Options.IOptions<NotificationSenderFactoryConfig> config)
        {
            this._serviceProvider = serviceProvider;
            // Fill in the factory accessor map with the actual objects of the diagram node types providers
            this._accessorsMap = new Dictionary<NotificationType, Func<INotificationSender>>();
            foreach (KeyValuePair<NotificationType, Type> pair in config?.Value?.Accessors)
            {
                this._accessorsMap.Add(pair.Key, () =>
                {
                    INotificationSender obj = this._serviceProvider.GetRequiredService(pair.Value) as INotificationSender;
                    return obj;
                });
            }
        }
        // UnSafe access to providers
        public INotificationSender Sender(NotificationType type)
        {
            if (this._accessorsMap.TryGetValue(type, out Func<INotificationSender> obj)) return obj();
            throw new ApplicationException("unrecognized form helper " + type.ToString());
        }
        // Safe access to providers
        public INotificationSender SenderSafe(NotificationType type)
        {
            if (this._accessorsMap.TryGetValue(type, out Func<INotificationSender> obj)) return obj();
            return null;
        }
        // Access with array like syntax
        public INotificationSender this[NotificationType key]
        {
            get
            {
                Func<INotificationSender> obj = null;
                if (this._accessorsMap.TryGetValue(key, out obj)) return obj();
                throw new Exception("unrecognized form helper " + key.ToString());
            }
        }
    }
}
