namespace Pawfect_API.Data.Entities.Types.Cache
{
	public class CacheConfig
	{
		public int TokensCacheTime { get; set; }
        public int ShelterDataCacheTime { get; set; }
        public int JWTTokensCacheTime { get; set; }
        public int RequirementResultTime { get; set; }
        public int QueryCacheTime { get; set; }
        public int AdminVerificationCacheTime { get; set; }

    }
}
