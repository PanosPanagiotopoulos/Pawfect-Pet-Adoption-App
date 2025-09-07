using Microsoft.Extensions.Options;
using Pawfect_Messenger.Data.Entities.Types.Authorisation;
using System.Security.Claims;

namespace Pawfect_Messenger.Services.AuthenticationServices
{
    public class PermissionPolicyProvider
    {
        private readonly PermissionPolicyProviderConfig _config;
        private readonly ClaimsExtractor _claimsExtractor;

        public PermissionPolicyProvider
        (
            IOptions<PermissionPolicyProviderConfig> config,
            ClaimsExtractor claimsExtractor
        )
        {
            _config = config.Value;
            _claimsExtractor = claimsExtractor;
        }

        public IEnumerable<String> GetAllPermissions() => _config.Policies.Select(p => p.Permission).Distinct();
        public Policy FindPolicy(String permission) => _config.Policies.FirstOrDefault(p => p.Permission == permission);

        public IEnumerable<String> GetAllRoles() => _config.Policies.SelectMany(p => p.Roles.Concat(p.AffiliatedRoles ?? Enumerable.Empty<String>())).Distinct();

        public IEnumerable<Policy> GetAllPolicies() => _config.Policies;

        public Boolean HasPermission(ClaimsPrincipal principal, String permission) => HasPermission(_claimsExtractor.CurrentUserRoles(principal), permission);

        public Boolean HasPermission(IEnumerable<String> userRoles, String permission)
        {
            Policy policy = FindPolicy(permission);
            return policy != null && policy.Roles.Concat(policy.AffiliatedRoles ?? []).Any(r => userRoles.Contains(r));
        }

        public Boolean HasAnyPermission(IEnumerable<String> userRoles, IEnumerable<String> permissions)
        {
            return _config.Policies
                .Where(p => permissions.Contains(p.Permission))
                .Any(p => p.Roles.Concat(p.AffiliatedRoles ?? []).Any(r => userRoles.Contains(r)));
        }

        public IEnumerable<String> GetPermissionsAndAffiliatedForRoles(IEnumerable<String> userRoles)
        {
            return _config.Policies
                .Where(p => p.Roles.Any(r => userRoles.Contains(r)) ||  p.AffiliatedRoles != null && p.AffiliatedRoles.Any(afRole => userRoles.Contains(afRole))  ) 
                .Select(p => p.Permission)
                .Distinct();
        }

        public IEnumerable<String> GetPermissionsForRoles(IEnumerable<String> userRoles)
        {
            return _config.Policies
                .Where(p => p.Roles.Any(r => userRoles.Contains(r)))
                .Select(p => p.Permission)
                .Distinct();
        }

        public IEnumerable<String> GetAffiliatedRolesForPermission(String permission) => FindPolicy(permission)?.AffiliatedRoles ?? Enumerable.Empty<String>();

        public IEnumerable<String> GetAllAffiliatedRolesForPermission(String permission)
        {
            return GetAffiliatedRolesForPermission(permission).Distinct();
        }

        public IEnumerable<String> GetAllAffiliatedRolesForPermissions(IEnumerable<String> permissions)
        {
            return _config.Policies
                .Where(p => permissions.Contains(p.Permission))
                .SelectMany(p => p.AffiliatedRoles ?? Enumerable.Empty<String>())
                .Distinct();
        }

        public IEnumerable<String> GetAllAffiliatedRoles()
        {
            return _config.Policies
                .SelectMany(p => p.AffiliatedRoles ?? Enumerable.Empty<String>())
                .Distinct();
        }

    }
}