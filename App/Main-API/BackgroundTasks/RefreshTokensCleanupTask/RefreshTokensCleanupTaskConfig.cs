namespace Main_API.BackgroundTasks.RefreshTokensCleanupTask
{
    public class RefreshTokensCleanupTaskConfig
    {
        public Boolean Enable { get; set; }
        // Periodicity
        public int WakeUpAfterSeconds { get; set; }

        // Batch size to delete big amounts of messages in parts to not hold the transaction locked for too long
        public int BatchSize { get; set; }
    }
}
