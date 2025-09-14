using Microsoft.AspNetCore.Authorization;
using Pawfect_Messenger.Data.Entities.Types.Authorisation;
using Pawfect_Messenger.Exceptions;
using System.Security.Claims;

namespace Pawfect_Messenger.Services.AuthenticationServices
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly IAuthorizationContentResolver _authorizationContentResolver;
        private readonly Microsoft.AspNetCore.Authorization.IAuthorizationService _authorizationService; 
        private readonly PermissionPolicyProvider _permissionProvider; 
        private readonly ClaimsExtractor _claimsExtractor;

        public AuthorizationService
        (
            IAuthorizationContentResolver AuthorizationContentResolver,
            Microsoft.AspNetCore.Authorization.IAuthorizationService authorizationService,
            PermissionPolicyProvider permissionProvider,
            ClaimsExtractor claimsExtractor
        )
        {
            _authorizationContentResolver = AuthorizationContentResolver;
            _authorizationService = authorizationService;
            _permissionProvider = permissionProvider;
            _claimsExtractor = claimsExtractor;
        }
        public async Task<Boolean> AuthorizeAsync(params String[] permissions)
        {
            if (permissions == null || permissions.Length == 0)
                throw new ArgumentException("No permissions provided to check.");

            ClaimsPrincipal user = _authorizationContentResolver.CurrentPrincipal();
            if (user == null) throw new ForbiddenException("User is not authenticated.");
                
            List<String> userRoles = _claimsExtractor.CurrentUserRoles(user) ?? new List<String>();

            return await Task.FromResult(_permissionProvider.HasAnyPermission(userRoles, permissions));
        }

        public async Task<Boolean> AuthorizeAffiliatedAsync(AffiliatedResource resource, params String[] permissions)
        {
            if (resource == null || resource.AffiliatedFilterParams == null)
                throw new ArgumentException("Invalid affiliated resource provided.");

            ClaimsPrincipal user = _authorizationContentResolver.CurrentPrincipal();
            if (user == null) throw new ForbiddenException("User is not authenticated.");

            resource.AffiliatedRoles = _authorizationContentResolver.AffiliatedRolesOf(permissions);

            AuthorizationResult authorizationResult = await _authorizationService.AuthorizeAsync(user, resource, new AffiliatedRequirement(resource));
            return authorizationResult.Succeeded;
        }

        public async Task<Boolean> AuthorizeOwnedAsync(OwnedResource resource)
        {
            if (resource == null || resource.OwnedFilterParams == null)
                throw new ArgumentException("Invalid affiliated resource provided.");

            ClaimsPrincipal user = _authorizationContentResolver.CurrentPrincipal();
            if (user == null) throw new ForbiddenException("User is not authenticated.");

            AuthorizationResult authorizationResult = await _authorizationService.AuthorizeAsync(user, resource, new OwnedRequirement(resource));
            return authorizationResult.Succeeded;
        }

        public async Task<Boolean> AuthorizeOrAffiliatedAsync(AffiliatedResource resource, params String[] permissions)
        {
            Boolean isAuthorised = await AuthorizeAsync(permissions);
            if (isAuthorised) return true;

            Boolean isAffiliated = await AuthorizeAffiliatedAsync(resource, permissions);
            return isAffiliated;
        }

        public async Task<Boolean> AuthorizeOrOwnedAsync(OwnedResource resource, params String[] permissions)
        {
            Boolean isAuthorised = await AuthorizeAsync(permissions);
            if (isAuthorised) return true;

            Boolean isOwned = await AuthorizeOwnedAsync(resource);
            return isOwned;
        }

        public async Task<Boolean> AuthorizeOrOwnedOrAffiliated(AuthContext context, params String[] permissions)
        {
            Boolean isAuthorised = await AuthorizeAsync(permissions);
            if (isAuthorised) return true;

            if (context.AffiliatedResource != null)
            {
                Boolean isAffiliated = await this.AuthorizeAffiliatedAsync(context.AffiliatedResource, permissions);
                if (isAffiliated) return isAffiliated;
            }

            if (context.OwnedResource != null)
            {
                Boolean isOwned = await this.AuthorizeOwnedAsync(context.OwnedResource);
                return isOwned;
            }

            return false;
        }

        public async Task<Boolean> AuthorizeOrOwnedOrAffiliated(
            OwnedResource ownedResource,
            AffiliatedResource affiliatedResource,
            params String[] permissions)
        {
            Boolean isAuthorised = await AuthorizeAsync(permissions);
            if (isAuthorised) return true;

            Boolean isAffiliated = await AuthorizeAffiliatedAsync(affiliatedResource, permissions);
            if (isAffiliated) return isAffiliated;

            Boolean isOwned = await AuthorizeOwnedAsync(ownedResource);
            return isOwned;
        }
    }
}