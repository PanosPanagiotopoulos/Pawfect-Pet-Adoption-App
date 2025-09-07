using Pawfect_Messenger.DevTools;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Pawfect_Messenger.Services.AuthenticationServices
{
    public class ClaimsExtractor
    {
        public String CurrentUserId(ClaimsPrincipal user)
        {
            return user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public String CurrentUserEmail(ClaimsPrincipal user)
        {
            return user?.FindFirst(ClaimTypes.Email)?.Value;
        }

        public DateTime CurrentUserLoggedAtDate(ClaimsPrincipal user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            String iatValue = user.FindFirst("iat")?.Value;
            if (String.IsNullOrEmpty(iatValue))
                throw new InvalidOperationException("Token does not contain issued at time");

            if (!long.TryParse(iatValue, out long iatUnix))
                throw new InvalidOperationException("Invalid issued at time format");

            try
            {
                DateTime issuedAt = DateTimeOffset.FromUnixTimeSeconds(iatUnix).UtcDateTime;
                return issuedAt;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to parse issued at time", ex);
            }
        }

        public List<String> CurrentUserRoles(ClaimsPrincipal user)
        {
            return [.. user?.FindAll(ClaimTypes.Role).Select(c => c.Value)];
        }
    }
}
