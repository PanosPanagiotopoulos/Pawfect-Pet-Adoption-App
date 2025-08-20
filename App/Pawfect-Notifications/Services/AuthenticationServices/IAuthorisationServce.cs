using Pawfect_Notifications.Data.Entities.Types.Authorization;
using System.Security.Claims;

namespace Pawfect_Notifications.Services.AuthenticationServices
{
    public interface IAuthorizationService
    {
        Task<Boolean> AuthorizeAsync(params String[] permissions);
        Task<Boolean> AuthorizeOwnedAsync(OwnedResource resource);
        Task<Boolean> AuthorizeAffiliatedAsync(AffiliatedResource resource, params String[] permissions);

        Task<Boolean> AuthorizeOrAffiliatedAsync(AffiliatedResource resource, params String[] permissions);

        Task<Boolean> AuthorizeOrOwnedAsync(OwnedResource resource, params String[] permissions);

        Task<Boolean> AuthorizeOrOwnedOrAffiliated(AuthContext context, params String[] permissions);
        Task<Boolean> AuthorizeOrOwnedOrAffiliated(
            OwnedResource ownedResource,
            AffiliatedResource affiliatedResource,
            params String[] permissions);
    }
}