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

		public JwtService
		(
			IOptions<JwtConfig> jwtConfiguration,
			IOptions<CacheConfig> cacheConfiguration,
			ILogger<JwtService> logger,
			IMemoryCache memoryCache
		)
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
		/// <param name="role">Ο ρόλος του χρήστη.</param>
		/// <returns>Ένα JWT token σε μορφή String</returns>
		public String? GenerateJwtToken(String userId, String email, String role, String isEmailVerified, String isVerified)
		{
			String? issuer = _jwtConfiguration.Issuer; // Εκδότης του token
			List<String>? audiences = _jwtConfiguration.Audiences; // Αποδέκτης του token
			String? jwtKey = _jwtConfiguration.Key; // Μυστικό Κλειδί JWT token

			if (String.IsNullOrEmpty(issuer) || audiences == null || !audiences.Any() || String.IsNullOrEmpty(jwtKey))
			{
				// LOGS //
				_logger.LogError("Δεν βρέθηκε configuration για τον εκδότη ή τον αποδέκτη του JWT ή του μυστικού κλειδιού JWT");
				return null;
			}

			// Δημιουργεί τις απαιτούμενες δηλώσεις (claims) για το JWT
			List<Claim> claims = new List<Claim>
		{
			new Claim(ClaimTypes.NameIdentifier, userId), // Αναγνωριστικό χρήστη
            new Claim(JwtRegisteredClaimNames.Email, email), // Email χρήστη
			new Claim("isEmailVerified", isEmailVerified), // Flag επιβεβαιωσης χρηστη
			new Claim("isVerified", isVerified), // Flag επιβεβαιωσης χρηστη
			new Claim(ClaimTypes.Role, role), // Ρόλος χρήστη
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique Id του token 
        };

			// Δημιουργεί το κλειδί ασφαλείας από τις ρυθμίσεις
			SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
			// Ορίζει την κρυπτογράφηση HMAC-SHA256
			SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			// Add multiple audiences as a custom claim
			foreach (String audience in audiences)
			{
				claims.Add(new Claim(JwtRegisteredClaimNames.Aud, audience));
			}

			// Δημιουργεί το JWT token
			JwtSecurityToken token = new JwtSecurityToken(
				issuer: issuer,
				claims: claims, // Δηλώσεις που περιλαμβάνονται στο token
				expires: DateTime.UtcNow.AddMinutes(_cacheConfiguration.TokensCacheTime), // Λήξη του token (σε 1 ώρα)
				signingCredentials: creds // Διαπιστευτήρια κρυπτογράφησης
			);

			// Επιστρέφει το token σε μορφή String
			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		/// <summary>
		/// Ακυρώνει το token.
		/// </summary>
		/// <param name="tokenId">Το μοναδικό αναγνωριστικό του token.</param>
		/// <param name="expiration">Η ημερομηνία λήξης του token.</param>
		public void RevokeToken(String tokenId, DateTime expiration)
		{
			_memoryCache.Set(tokenId, true, expiration - DateTime.UtcNow);
		}

		/// <summary>
		/// Ελέγχει εάν το token έχει ακυρωθεί.
		/// </summary>
		/// <param name="tokenId">Το μοναδικό αναγνωριστικό του token.</param>
		/// <returns>Επιστρέφει true εάν το token έχει ακυρωθεί, αλλιώς false.</returns>
		public Boolean IsTokenRevoked(String tokenId)
		{
			return _memoryCache.TryGetValue(tokenId, out _);
		}

		/// <summary>
		/// Δημιουργεί το μυστικό κλειδί (secret key) για το JWT χρησιμοποιώντας έναν τυχαίο αλγόριθμο.
		/// </summary>
		/// <returns>Μυστικό κλειδί σε μορφή Base64 String</returns>
		private String GenerateJwtSecretKey()
		{
			// Δημιουργεί ένα κλειδί 256-bit (32 bytes)
			byte[] key = new byte[32];
			RandomNumberGenerator.Fill(key); // Γεμίζει το κλειδί με τυχαία δεδομένα

			// Επιστρέφει το κλειδί σε μορφή Base64 String
			return Convert.ToBase64String(key);
		}

		public static JwtConfig GetJwtSettings(IConfiguration configuration)
		{
			JwtConfig? jwtSettings = configuration.GetSection("Jwt").Get<JwtConfig>();

			if (String.IsNullOrEmpty(jwtSettings.Key))
			{
				throw new ArgumentException("JWT Key δεν βρεθηκε.");
			}

			if (String.IsNullOrEmpty(jwtSettings.Issuer))
			{
				throw new ArgumentException("JWT Issuer δεν βρεθηκε.");
			}

			if (jwtSettings.Audiences == null || !jwtSettings.Audiences.Any())
			{
				throw new ArgumentException("JWT audiencess δεν βρεθηκε.");
			}

			return jwtSettings;
		}
	}
}