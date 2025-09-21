using Microsoft.Extensions.Options;
using Pawfect_Notifications.Data.Entities.EnumTypes;
using Pawfect_Notifications.Data.Entities.Types.Authorization;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Pawfect_Notifications.Services.AuthenticationServices
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

        public Boolean HasPermission(ClaimsPrincipal principal, String permission) => this.HasPermission(_claimsExtractor.CurrentUserRoles(principal), permission);

        public Boolean HasPermission(IEnumerable<String> userRoles, String permission)
        {
            Policy policy = this.FindPolicy(permission);
            return policy != null && policy.Roles.Any(r => userRoles.Contains(r));
        }

        public Boolean HasAnyPermission(IEnumerable<String> userRoles, IEnumerable<String> permissions)
        {
            return _config.Policies
                .Where(p => permissions.Contains(p.Permission))
                .Any(p => p.Roles.Any(r => userRoles.Contains(r)));
        }

        public IEnumerable<String> GetPermissionsAndAffiliatedForRoles(IEnumerable<String> userRoles)
        {
            return _config.Policies
                .Where(p => p.Roles.Any(r => userRoles.Contains(r)) || ( p.AffiliatedRoles != null && p.AffiliatedRoles.Any(afRole => userRoles.Contains(afRole)) ) ) 
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

        public IEnumerable<String> GetAffiliatedRolesForPermission(String permission) => this.FindPolicy(permission)?.AffiliatedRoles ?? Enumerable.Empty<String>();

        public IEnumerable<String> GetAllAffiliatedRolesForPermission(String permission)
        {
            return this.GetAffiliatedRolesForPermission(permission).Distinct();
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