using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices
{
    public class ClaimsExtractor
    {
        public String CurrentUserId(ClaimsPrincipal user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            return user.FindFirst(JwtRegisteredClaimNames.NameId)?.Value;
        }

        public String CurrentUserEmail(ClaimsPrincipal user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            return user.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
        }

        public List<String> CurrentUserRoles(ClaimsPrincipal user)
        {
            return [.. user.FindAll(ClaimTypes.Role).Select(c => c.Value)];
        }
    }
}
