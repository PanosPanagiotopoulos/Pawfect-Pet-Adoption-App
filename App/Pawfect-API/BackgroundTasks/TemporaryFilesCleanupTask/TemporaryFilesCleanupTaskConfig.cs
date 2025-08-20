namespace Pawfect_API.BackgroundTasks.TemporaryFilesCleanupTask
{
    public class TemporaryFilesCleanupTaskConfig
    {
        public Boolean Enable { get; set; }
        // Periodicity
        public int WakeUpAfterSeconds { get; set; }

        // Batch size to delete big amounts of messages in parts to not hold the transaction locked for too long
        public int BatchSize { get; set; }

        public int MinutesPrior { get; set; }
    }
}
