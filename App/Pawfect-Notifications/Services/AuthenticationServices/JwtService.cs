using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Pawfect_Notifications.Data.Entities.Types.Authentication;
using Pawfect_Notifications.Data.Entities.Types.Cache;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace Pawfect_Notifications.Services.AuthenticationServices
{
    /// <summary>
    /// Μοναδική (singleton) υπηρεσία για τη διαχείριση του authentication
    /// </summary>
    public class JwtService
    {
        /// <summary>
        /// Η διαμόρφωση της εφαρμογής (ρυθμίσεις από appsettings.json)
        /// </summary>
        private readonly JwtConfig _jwtConfiguration;
        private readonly CacheConfig _cacheConfiguration;
        private readonly ILogger<JwtService> _logger;
        private readonly IMemoryCache _memoryCache;

        public JwtService(
            IOptions<JwtConfig> jwtConfiguration,
            IOptions<CacheConfig> cacheConfiguration,
            ILogger<JwtService> logger,
            IMemoryCache memoryCache)
        {
            _jwtConfiguration = jwtConfiguration.Value;
            _cacheConfiguration = cacheConfiguration.Value;
            _logger = logger;
            _memoryCache = memoryCache;
        }

        public int JwtExpireAfterMinutes { get { return this._cacheConfiguration.JWTTokensCacheTime; } }

        public static String ACCESS_TOKEN = "at_ppa";
        public static String REFRESH_TOKEN = "rt_ppa";

        public void RevokeToken(String tokenId, DateTime expiresAt)
        {
            if (String.IsNullOrEmpty(tokenId)) return;

            _memoryCache.Set(
                key: $"revoked_token_{tokenId}",
                value: true,
                options: new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = expiresAt
                });
        }

        /// <summary>
        /// Ελέγχει εάν το token έχει ακυρωθεί.
        /// </summary>
        /// <param name="tokenId">Το μοναδικό αναγνωριστικό του token.</param>
        /// <returns>Επιστρέφει true εάν το token έχει ακυρωθεί, αλλιώς false.</returns>
        public Boolean IsTokenRevoked(String token)
        {
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

            if (!handler.CanReadToken(token))
            {
                _logger.LogWarning("Το token δεν μπορεί να διαβαστεί.");
                return true;
            }

            JwtSecurityToken jwtToken = handler.ReadJwtToken(token);

            return this.IsTokenRevoked(jwtToken);
        }

        public Boolean IsTokenRevoked(JwtSecurityToken token)
        {
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

            DateTime expiration = token.ValidTo;

            if (expiration <= DateTime.UtcNow)
            {
                _logger.LogWarning("Το token έχει λήξει στις {Expiration}.", expiration);
                return true;
            }

            return false;
        }


        /// <summary>
        /// Δημιουργεί το μυστικό κλειδί (secret key) για το JWT χρησιμοποιώντας έναν τυχαίο αλγόριθμο.
        /// </summary>
        /// <returns>Μυστικό κλειδί σε μορφή Base64 String</returns>
        private String GenerateJwtSecretKey()
        {
            byte[] key = new byte[32];
            RandomNumberGenerator.Fill(key);
            String secretKey = Convert.ToBase64String(key);
            _logger.LogInformation("Δημιουργήθηκε νέο μυστικό κλειδί JWT.");
            return secretKey;
        }

        public static JwtConfig GetJwtSettings(IConfiguration configuration)
        {
            JwtConfig? jwtSettings = configuration.GetSection("Jwt").Get<JwtConfig>();

            if (String.IsNullOrEmpty(jwtSettings.Key))
                throw new ArgumentException("JWT Key δεν βρέθηκε.");

            if (String.IsNullOrEmpty(jwtSettings.Issuer))
                throw new ArgumentException("JWT Issuer δεν βρέθηκε.");

            if (jwtSettings.Audiences == null || !jwtSettings.Audiences.Any())
                throw new ArgumentException("JWT audiences δεν βρέθηκαν.");

            return jwtSettings;
        }
    }
}