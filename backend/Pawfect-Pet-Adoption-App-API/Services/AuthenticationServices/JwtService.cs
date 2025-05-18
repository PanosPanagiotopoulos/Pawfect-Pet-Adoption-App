using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authentication;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Cache;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices
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
        public String? GenerateJwtToken(String userId, String email, List<String> roles, String isEmailVerified, String isVerified)
        {
            String? issuer = _jwtConfiguration.Issuer;
            List<String>? audiences = _jwtConfiguration.Audiences;
            String? jwtKey = _jwtConfiguration.Key;

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

            if (roles == null || !roles.Any())
            {
                _logger.LogError("Η λίστα των ρόλων είναι κενή ή null για το userId: {UserId}.", userId);
                return null;
            }
           
            // Δημιουργεί τις απαιτούμενες δηλώσεις (claims) για το JWT
            List<Claim> claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.NameId, userId),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim("isEmailVerified", isEmailVerified),
                new Claim("isVerified", isVerified),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Add multiple roles as individual claims
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

        /// <summary>
        /// Ακυρώνει το token.
        /// </summary>
        /// <param name="tokenId">Το μοναδικό αναγνωριστικό του token.</param>
        /// <param name="expiration">Η ημερομηνία λήξης του token.</param>
        public void RevokeToken(String tokenId, DateTime expiration)
        {
            TimeSpan relativeExpiration = expiration - DateTime.UtcNow;

            if (relativeExpiration <= TimeSpan.Zero)
            {
                _logger.LogError("Attempted to revoke a token that has already expired. TokenId: {TokenId}", tokenId);
                return;
            }

            _memoryCache.Set(tokenId, true, relativeExpiration);
            _logger.LogInformation("Ακυρώθηκε το token με TokenId: {TokenId}.", tokenId);
        }

        /// <summary>
        /// Ελέγχει εάν το token έχει ακυρωθεί.
        /// </summary>
        /// <param name="tokenId">Το μοναδικό αναγνωριστικό του token.</param>
        /// <returns>Επιστρέφει true εάν το token έχει ακυρωθεί, αλλιώς false.</returns>
        public Boolean IsTokenRevoked(String tokenId)
        {
            Boolean isRevoked = _memoryCache.TryGetValue(tokenId, out _);
            if (isRevoked)
            {
                _logger.LogWarning("Το token με TokenId: {TokenId} έχει ακυρωθεί.", tokenId);
            }
            return isRevoked;
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