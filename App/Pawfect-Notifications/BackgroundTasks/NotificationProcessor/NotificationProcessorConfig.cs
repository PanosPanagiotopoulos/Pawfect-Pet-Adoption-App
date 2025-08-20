using Pawfect_Notifications.Data.Entities.EnumTypes;

namespace Pawfect_Notifications.BackgroundTasks.NotificationProcessor
{
    public class NotificationProcessorConfig
    {
        public Boolean Enable { get; set; } 
        public Int32 IntervalSeconds { get; set; } 
        public Int32 BatchSize { get; set; } 
        public NotificationTypeOptions DefaultOptions { get; set; }
    }

    public class NotificationTypeOptions
    {
        public Int32 RetryDelayStepSeconds { get; set; }
        public Int32 MaxRetryDelaySeconds { get; set; } 
        public Int32 TooOldToProcessSeconds { get; set; } 
        public Boolean EnableProcessing { get; set; } = true;
    }
}
