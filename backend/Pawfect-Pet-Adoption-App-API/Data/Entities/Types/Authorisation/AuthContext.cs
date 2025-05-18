using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices;
using System.Security.Claims;

namespace Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation
{
    public class AuthContext
    {
        public OwnedResource OwnedResource { get; private set; }
        public AffiliatedResource AffiliatedResource { get; private set; }
        public String CurrentUserId { get; private set; }

        public AuthContext
        (
            String currentUserId,
            OwnedResource ownedResource = null, 
            AffiliatedResource affiliatedResource = null
        )
        {
            this.OwnedResource = ownedResource;
            this.AffiliatedResource = affiliatedResource;
            this.CurrentUserId = currentUserId;
        }
    }

    public class AuthContextBuilder
    {
        private readonly IAuthorisationContentResolver _authorisationContentResolver;
        private readonly ClaimsExtractor _claimsExtractor;

        private OwnedResource _ownedResource { get; set; }
        private AffiliatedResource _affiliatedResource { get; set; }

        public AuthContextBuilder
        (
            IAuthorisationContentResolver authorisationContentResolver,
            ClaimsExtractor claimsExtractor
        )
        {
            this._authorisationContentResolver = authorisationContentResolver;
            this._claimsExtractor = claimsExtractor;
        }

        public AuthContextBuilder OwnedFrom(OwnedResource ownedResource) { this._ownedResource = ownedResource; return this; }
        public AuthContextBuilder OwnedFrom(Lookup requestedOwnedFilters, String ownedById) => this.OwnedFrom(requestedOwnedFilters, new List<String>() { ownedById });
        public AuthContextBuilder OwnedFrom(Lookup requestedOwnedFilters, List<String> ownedByIds = null) { _ownedResource = _authorisationContentResolver.BuildOwnedResource(requestedOwnedFilters, ownedByIds); return this; }
        public AuthContextBuilder OwnedFrom(AffiliatedResource affiliatedResource) { this._affiliatedResource = affiliatedResource; return this; }
        public AuthContextBuilder AffiliatedWith(Lookup requestedAffiliatedFilters, List<String> affiliatedRoles = null, String affiliatedId = null) { _affiliatedResource = _authorisationContentResolver.BuildAffiliatedResource(requestedAffiliatedFilters, affiliatedRoles); return this; }

        public AuthContext Build()
        {
            ClaimsPrincipal user = _authorisationContentResolver.CurrentPrincipal();
            if (user == null) throw new ArgumentException("User is not authenticated.");
            String userId = _claimsExtractor.CurrentUserId(user);
            if (userId == null) throw new ArgumentException("User ID is not found in claims.");

            return new AuthContext
            (
                userId,
                _ownedResource,
                _affiliatedResource
            );
        }
    }
}
