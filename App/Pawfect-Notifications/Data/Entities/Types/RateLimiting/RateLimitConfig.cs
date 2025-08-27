namespace Pawfect_Notifications.Data.Entities.Types.RateLimiting
{
    public class RateLimitConfig
    {
        public RateLimitTier Permissive { get; set; }
        public RateLimitTier Moderate { get; set; } 
        public RateLimitTier Restrictive { get; set; }
        public RateLimitTier Strict { get; set; } 
        public Boolean EnableGlobalRateLimit { get; set; }
        public int GlobalRequestsPerMinute { get; set; }
    }

    public class RateLimitTier
    {
        public int RequestsPerMinute { get; set; }
        public int BurstLimit { get; set; }
        public TimeSpan WindowSize { get; set; } 
        public Boolean Enabled { get; set; } 
    }
}
