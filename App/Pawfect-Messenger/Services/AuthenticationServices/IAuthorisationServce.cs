using Pawfect_Messenger.Data.Entities.Types.Authorisation;

namespace Pawfect_Messenger.Services.AuthenticationServices
{
    public interface IAuthorizationService
    {
        Task<bool> AuthorizeAsync(params String[] permissions);
        Task<bool> AuthorizeOwnedAsync(OwnedResource resource);
        Task<bool> AuthorizeAffiliatedAsync(AffiliatedResource resource, params String[] permissions);

        Task<bool> AuthorizeOrAffiliatedAsync(AffiliatedResource resource, params String[] permissions);

        Task<bool> AuthorizeOrOwnedAsync(OwnedResource resource, params String[] permissions);

        Task<bool> AuthorizeOrOwnedOrAffiliated(AuthContext context, params String[] permissions);
        Task<bool> AuthorizeOrOwnedOrAffiliated(
            OwnedResource ownedResource,
            AffiliatedResource affiliatedResource,
            params String[] permissions);
    }
}