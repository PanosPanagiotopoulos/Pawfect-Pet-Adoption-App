using Microsoft.Extensions.Caching.Memory;
using Pawfect_Messenger.Data.Entities.EnumTypes;
using Pawfect_Messenger.Models.User;
using Pawfect_Messenger.Services.Convention;

namespace Pawfect_Messenger.Services.PresenceServices
{
    public class PresenceService : IPresenceService
    {
        private readonly IMemoryCache _cache;
        private readonly IConventionService _convention;

        private static String Key(String userId) => $"presence:{userId}";

        public PresenceService
        (
            IMemoryCache cache,
            IConventionService convention
        )
        {
            _cache = cache;
            _convention = convention;
        }

        public async Task MarkOnline(String userId, String connectionId)
        {
            if (!_convention.IsValidId(userId)) return;

            HashSet<String> set = _cache.GetOrCreate(Key(userId), _ => new HashSet<String>());
            Boolean wasOffline = set.Count == 0;
            set.Add(connectionId);
            _cache.Set(Key(userId), set);
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
                    _cache.Set(Key(userId), set);
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
