using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Pawfect_API.Data.Entities;
using Pawfect_API.Data.Entities.Types.Authentication;
using Pawfect_API.Data.Entities.Types.Cache;
using Pawfect_API.Models.User;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Pawfect_API.Services.AuthenticationServices
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

        /// <summary>
        /// Δημιουργεί το JWT token.
        /// </summary>
        /// <param name="userId">Το μοναδικό αναγνωριστικό του χρήστη.</param>
        /// <param name="email">Το email του χρήστη.</param>
        /// <param name="roles">Η λίστα των ρόλων του χρήστη.</param>
        /// <param name="affiliatedRoles">Η λίστα των συνδεδεμένων ρόλων του χρήστη.</param>
        /// <param name="isEmailVerified">Flag επιβεβαίωσης email.</param>
        /// <param name="isVerified">Flag επιβεβαίωσης χρήστη.</param>
        /// <returns>Ένα JWT token σε μορφή String ή null αν αποτύχει.</returns>
        public String? GenerateJwtToken(String userId, String email, List<String> roles, Boolean isVerified)
        {
            String issuer = _jwtConfiguration.Issuer;
            List<String> audiences = _jwtConfiguration.Audiences;
            String jwtKey = _jwtConfiguration.Key;

            if (String.IsNullOrEmpty(issuer) || audiences == null || !audiences.Any() || String.IsNullOrEmpty(jwtKey))
            {
                _logger.LogError("Δεν βρέθηκε configuration για τον εκδότη, τον αποδέκτη ή το μυστικό κλειδί του JWT.");
                return null;
            }

            if (String.IsNullOrEmpty(userId) || String.IsNullOrEmpty(email))
            {
                _logger.LogError("Το userId ή το email είναι κενό ή null.");
                return null;
            }

            // Δημιουργεί τις απαιτούμενες δηλώσεις (claims) για το JWT
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email),
                new Claim("jti", Guid.NewGuid().ToString()),
                new Claim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim("isVerified", isVerified.ToString(), ClaimValueTypes.Boolean)
            };

            // Add multiple roles 
            foreach (String role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Add multiple audiences as individual claims
            foreach (String audience in audiences)
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.Aud, audience));
            }

            // Δημιουργεί το κλειδί ασφαλείας από τις ρυθμίσεις
            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Δημιουργεί το JWT token
            JwtSecurityToken token = new JwtSecurityToken(
                issuer: issuer,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_cacheConfiguration.JWTTokensCacheTime),
                signingCredentials: creds
            );

            String jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            return jwtToken;
        }

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

        public RefreshToken GenerateRefreshToken(String userId, String ip)
        {
            String token = this.GenerateJwtSecretKey();

           return new Data.Entities.RefreshToken
            {
                Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString(),
                Token = token,
                LinkedTo = userId,
                Ip = ip,
                ExpiresAt = DateTime.UtcNow.AddHours(_jwtConfiguration.RefreshTokenExpiration),
                CreatedAt = DateTime.UtcNow
            };
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