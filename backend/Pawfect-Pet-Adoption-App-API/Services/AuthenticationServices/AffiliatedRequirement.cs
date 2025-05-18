using Microsoft.AspNetCore.Authorization;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
using Pawfect_Pet_Adoption_App_API.Services.Convention;
using Pawfect_Pet_Adoption_App_API.Services.FilterServices;
using MongoDB.Driver;
using Pawfect_Pet_Adoption_App_API.Services.MongoServices;

namespace Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices
{
    public class AffiliatedRequirement : IAuthorizationRequirement
    {
        public AffiliatedResource Resource { get; }

        public AffiliatedRequirement(AffiliatedResource resource)
        {
            Resource = resource;
        }
    }

    public class AffiliatedRequirementHandler<TEntity, TLookup> : AuthorizationHandler<AffiliatedRequirement>
        where TEntity : class
        where TLookup : Models.Lookups.Lookup
    {
        private readonly ClaimsExtractor _claimsExtractor;
        private readonly IFilterBuilder<TEntity, TLookup> _filterBuilder;
        private readonly IAuthorisationContentResolver _authorisationContentResolver;
        private readonly MongoDbService _mongoDbService;

        public AffiliatedRequirementHandler(
            ClaimsExtractor claimsExtractor,
            IFilterBuilder<TEntity, TLookup> filterBuilder,
            IAuthorisationContentResolver authorisationContentResolver,
            MongoDbService mongoDbService)
        {
            _claimsExtractor = claimsExtractor;
            _filterBuilder = filterBuilder;
            _authorisationContentResolver = authorisationContentResolver;
            _mongoDbService = mongoDbService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AffiliatedRequirement requirement)
        {
            AffiliatedResource affiliatedResource = requirement.Resource;
            if (affiliatedResource == null ||
                affiliatedResource.AffiliatedFilterParams == null)
            {
                context.Fail();
                return;
            }

            // Basic affiliation check up if requested
            if (affiliatedResource.AffiliatedRoles != null)
            {
                List<String> userRoles = _claimsExtractor.CurrentUserRoles(context.User) ?? new List<String>();

                Boolean hasAffiliatedRole = requirement.Resource.AffiliatedRoles != null && userRoles.Intersect(requirement.Resource.AffiliatedRoles).ToList().Count != 0;

                if (!hasAffiliatedRole)
                {
                    context.Fail();
                    return;
                }
            }
            
            // --

            // * Intenral affiliation lookup *

            // Validate that the lookup type matches TLookup
            if (affiliatedResource.AffiliatedFilterParams.RequestedFilters is not TLookup lookup)
                throw new ArgumentException("Invalid lookup type");

            // The requested filter from the user
            FilterDefinition<TEntity> filter = await _filterBuilder.Build((TLookup)lookup);

            if (filter == null) throw new InvalidOperationException("Failed to build filters from lookup");

            FilterDefinition<TEntity> requiredFilter = _authorisationContentResolver.BuildAffiliatedFilterParams<TEntity>(affiliatedResource.AffiliatedId);

            FilterDefinition<TEntity> combinedFilter = Builders<TEntity>.Filter.And(filter, requiredFilter);

            // Count matching documents asynchronously
            long count = await _mongoDbService.GetCollection<TEntity>().CountDocumentsAsync(combinedFilter);

            // Succeed if count > 0, otherwise fail
            if (count > 0) context.Succeed(requirement);
            else context.Fail();
        }
    }
}