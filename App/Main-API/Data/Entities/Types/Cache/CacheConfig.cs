namespace Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Cache
{
	public class CacheConfig
	{
		public int TokensCacheTime { get; set; }
        public int ShelterDataCacheTime { get; set; }
        public int JWTTokensCacheTime { get; set; }
        public int RequirementResultTime { get; set; }

    }
}
