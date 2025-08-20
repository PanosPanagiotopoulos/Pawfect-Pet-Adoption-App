namespace Pawfect_Notifications.Data.Entities.EnumTypes
{
    public enum NotificationStatus: short
    {
        Pending = 1,
        Processing = 2,
        Completed = 3,
        Failed = 4,
        Cancelled = 5,
        Scheduled = 6,
        Error = 7
    }
}
