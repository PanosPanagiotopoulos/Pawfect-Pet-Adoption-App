using Pawfect_API.Models.Lookups;
using Pawfect_API.Services.AuthenticationServices;
using System.Security.Claims;

namespace Pawfect_API.Data.Entities.Types.Authorization
{
    public class AuthContext
    {
        public OwnedResource OwnedResource { get; private set; }
        public AffiliatedResource AffiliatedResource { get; private set; }
        public String CurrentUserId { get; private set; }
        public String AffiliatedId { get; private set; }

        public AuthContext
        (
            String currentUserId,
            String affiliatedId = null,
            OwnedResource ownedResource = null, 
            AffiliatedResource affiliatedResource = null
        )
        {
            this.OwnedResource = ownedResource;
            this.AffiliatedResource = affiliatedResource;
            this.CurrentUserId = currentUserId;
            this.AffiliatedId = affiliatedId;
        }
    }

    public class AuthContextBuilder
    {
        private readonly IAuthorizationContentResolver _authorizationContentResolver;
        private readonly ClaimsExtractor _claimsExtractor;

        private OwnedResource _ownedResource { get; set; }
        private AffiliatedResource _affiliatedResource { get; set; }

        private String _affiliatedId { get; set; }

        public AuthContextBuilder
        (
            IAuthorizationContentResolver AuthorizationContentResolver,
            ClaimsExtractor claimsExtractor
        )
        {
            this._authorizationContentResolver = AuthorizationContentResolver;
            this._claimsExtractor = claimsExtractor;
        }

        public AuthContextBuilder OwnedFrom(OwnedResource ownedResource) { this._ownedResource = ownedResource; return this; }
        public AuthContextBuilder OwnedFrom(Lookup requestedOwnedFilters, String ownedById) => this.OwnedFrom(requestedOwnedFilters, new List<String>() { ownedById });
        public AuthContextBuilder OwnedFrom(Lookup requestedOwnedFilters, List<String> ownedByIds = null) { _ownedResource = _authorizationContentResolver.BuildOwnedResource(requestedOwnedFilters, ownedByIds); return this; }
        public AuthContextBuilder AffiliatedWith(AffiliatedResource affiliatedResource, String affiliatedId = null) { this._affiliatedResource = affiliatedResource; this._affiliatedId = affiliatedId; return this; }
        public AuthContextBuilder AffiliatedWith(Lookup requestedAffiliatedFilters, List<String> affiliatedRoles = null, String affiliatedId = null) { _affiliatedResource = _authorizationContentResolver.BuildAffiliatedResource(requestedAffiliatedFilters, affiliatedRoles); this._affiliatedId = affiliatedId; return this; }

        public AuthContext Build()
        {
            ClaimsPrincipal user = _authorizationContentResolver.CurrentPrincipal();
            if (user == null) throw new ArgumentException("User is not authenticated.");
            String userId = _claimsExtractor.CurrentUserId(user);
            if (userId == null) throw new ArgumentException("User ID is not found in claims.");

            return new AuthContext
            (
                userId,
                _affiliatedId,
                _ownedResource,
                _affiliatedResource
            );
        }
    }
}
