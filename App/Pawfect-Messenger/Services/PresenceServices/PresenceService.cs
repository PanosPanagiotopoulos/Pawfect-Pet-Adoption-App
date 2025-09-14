using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Pawfect_Messenger.Data.Entities.EnumTypes;
using Pawfect_Messenger.Data.Entities.Types.Cache;
using Pawfect_Messenger.Models.User;
using Pawfect_Messenger.Services.Convention;

namespace Pawfect_Messenger.Services.PresenceServices
{
    public class PresenceService : IPresenceService
    {
        private readonly IMemoryCache _cache;
        private readonly IConventionService _convention;
        private readonly CacheConfig _cacheConfig;

        private static String Key(String userId) => $"presence:{userId}";

        public PresenceService
        (
            IMemoryCache cache,
            IConventionService convention,
            IOptions<CacheConfig> options
        )
        {
            _cache = cache;
            _convention = convention;
            _cacheConfig = options.Value;
        }

        public async Task MarkOnline(String userId, String connectionId)
        {
            if (!_convention.IsValidId(userId)) return;

            HashSet<String> set = _cache.GetOrCreate(Key(userId), _ => new HashSet<String>());
            Boolean wasOffline = set.Count == 0;
            set.Add(connectionId);
            _cache.Set(Key(userId), set, TimeSpan.FromMinutes(_cacheConfig.TokensCacheTime));
        }

        public async Task MarkOffline(String userId, String connectionId)
        {
            if (!_convention.IsValidId(userId)) return;

            if (_cache.TryGetValue(Key(userId), out HashSet<String> set))
            {
                set.Remove(connectionId);
                if (set.Count == 0)
                {
                    _cache.Remove(Key(userId));
                }
                else
                {
                    _cache.Set(Key(userId), set, TimeSpan.FromMinutes(_cacheConfig.TokensCacheTime));
                }
            }
        }

        public Boolean IsOnline(String userId) => _cache.TryGetValue(Key(userId), out HashSet<String> set) && set.Count > 0;

        public async Task<UserPresence> GetPresence(String userId)
        {
            UserStatus status = this.IsOnline(userId) ? UserStatus.Online : UserStatus.Offline;
            return new UserPresence { UserId = userId, Status = status };
        }
    }
}
