using Microsoft.AspNetCore.Authorization;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
using Pawfect_Pet_Adoption_App_API.Exceptions;
using Pawfect_Pet_Adoption_App_API.Services.Convention;
using System.Globalization;
using System.Security.Claims;

namespace Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices
{
    public class AuthorisationService : IAuthorisationService
    {
        private readonly IAuthorisationContentResolver _authorisationContentResolver;
        private readonly IAuthorizationService _authorizationService; 
        private readonly PermissionPolicyProvider _permissionProvider; 
        private readonly ClaimsExtractor _claimsExtractor;

        public AuthorisationService
        (
            IAuthorisationContentResolver authorisationContentResolver,
            IAuthorizationService authorizationService,
            PermissionPolicyProvider permissionProvider,
            ClaimsExtractor claimsExtractor
        )
        {
            _authorisationContentResolver = authorisationContentResolver;
            _authorizationService = authorizationService;
            _permissionProvider = permissionProvider;
            _claimsExtractor = claimsExtractor;
        }
        public async Task<Boolean> AuthorizeAsync(params String[] permissions)
        {
            if (permissions == null || permissions.Length == 0)
                throw new ArgumentException("No permissions provided to check.");

            ClaimsPrincipal user = _authorisationContentResolver.CurrentPrincipal();
            if (user == null) throw new UnAuthenticatedException("User is not authenticated.");

            List<String> userRoles = _claimsExtractor.CurrentUserRoles(user) ?? new List<String>();

            return await Task.FromResult(_permissionProvider.HasAnyPermission(userRoles, permissions));
        }

        public async Task<Boolean> AuthorizeAffiliatedAsync(AffiliatedResource resource)
        {
            if (resource == null || !resource.AffiliatedRoles.Any() || resource.AffiliatedFilterParams == null)
                throw new ArgumentException("Invalid affiliated resource provided.");

            ClaimsPrincipal user = _authorisationContentResolver.CurrentPrincipal();
            if (user == null) throw new UnAuthenticatedException("User is not authenticated.");

            AuthorizationResult authorizationResult = await _authorizationService.AuthorizeAsync(user, resource, new AffiliatedRequirement(resource));
            return authorizationResult.Succeeded;
        }

        public async Task<Boolean> AuthorizeOwnedAsync(OwnedResource resource)
        {
            if (resource == null || resource.OwnedFilterParams == null)
                throw new ArgumentException("Invalid affiliated resource provided.");

            ClaimsPrincipal user = _authorisationContentResolver.CurrentPrincipal();
            if (user == null) throw new UnAuthenticatedException("User is not authenticated.");

            AuthorizationResult authorizationResult = await _authorizationService.AuthorizeAsync(user, resource, new OwnedRequirement(resource));
            return authorizationResult.Succeeded;
        }

        public async Task<Boolean> AuthorizeOrAffiliatedAsync(AffiliatedResource resource, params String[] permissions)
        {
            Boolean isAuthorised = await this.AuthorizeAsync(permissions);
            if (isAuthorised) return true;

            Boolean isAffiliated = await this.AuthorizeAffiliatedAsync(resource);
            return isAffiliated;
        }

        public async Task<Boolean> AuthorizeOrOwnedAsync(OwnedResource resource, params String[] permissions)
        {
            Boolean isAuthorised = await this.AuthorizeAsync(permissions);
            if (isAuthorised) return true;

            Boolean isOwned = await this.AuthorizeOwnedAsync(resource);
            return isOwned;
        }

        public async Task<Boolean> AuthorizeOrOwnedOrAffiliated(AuthContext context, params String[] permissions)
        {
            Boolean isAuthorised = await this.AuthorizeAsync(permissions);
            if (isAuthorised) return true;

            if (context.AffiliatedResource != null)
            {
                Boolean isAffiliated = await this.AuthorizeAffiliatedAsync(context.AffiliatedResource);
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
            Boolean isAuthorised = await this.AuthorizeAsync(permissions);
            if (isAuthorised) return true;

            Boolean isAffiliated = await this.AuthorizeAffiliatedAsync(affiliatedResource);
            if (isAffiliated) return isAffiliated;

            Boolean isOwned = await this.AuthorizeOwnedAsync(ownedResource);
            return isOwned;
        }
    }
}