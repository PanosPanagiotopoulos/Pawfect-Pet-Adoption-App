using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
using Pawfect_Pet_Adoption_App_API.Exceptions;
using Pawfect_Pet_Adoption_App_API.Services.Convention;
using Pawfect_Pet_Adoption_App_API.Services.FilterServices;
using Pawfect_Pet_Adoption_App_API.Services.MongoServices;

namespace Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices
{
    public class OwnedRequirement : IAuthorizationRequirement
    {
        public OwnedResource Resource { get; }

        public OwnedRequirement(OwnedResource resource)
        {
            Resource = resource;
        }
    }

    public class OwnedRequirementHandler<TEntity, TLookup> : AuthorizationHandler<OwnedRequirement>
        where TEntity : class
        where TLookup : Models.Lookups.Lookup
    {
        private readonly IAuthorisationContentResolver _authorisationContentResolver;
        private readonly IConventionService _conventionService;
        private readonly ClaimsExtractor _claimsExtractor;
        private readonly IFilterBuilder<TEntity, TLookup> _filterBuilder;
        private readonly MongoDbService _mongoDbService;

        public OwnedRequirementHandler
        (
            IAuthorisationContentResolver contentResolver,
            IConventionService conventionService,
            ClaimsExtractor claimsExtractor,
            IFilterBuilder<TEntity, TLookup> filterBuilder,
            MongoDbService mongoDbService)
        {
            _authorisationContentResolver = contentResolver;
            _conventionService = conventionService;
            _claimsExtractor = claimsExtractor;
            _filterBuilder = filterBuilder;
            _mongoDbService = mongoDbService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, OwnedRequirement requirement)
        {
            String userId = _claimsExtractor.CurrentUserId(context.User);
            if (!_conventionService.IsValidId(userId)) throw new UnAuthenticatedException("User is not authenticated.");

            OwnedResource ownedResource = requirement.Resource;
            if (ownedResource == null)
                throw new ArgumentException("No resource provided.");

            // Check if user ids where added (means we check via owner instantly)
            if (requirement.Resource.UserIds != null && requirement.Resource.UserIds.Count() > 0 && requirement.Resource.UserIds.Contains(userId))
            {
                context.Succeed(requirement);
                return;
            }

            if (ownedResource.OwnedFilterParams == null)
                throw new ArgumentException("No filter params provided.");

            // * Intenral affiliation lookup *

            // Validate that the lookup type matches TLookup
            if (ownedResource.OwnedFilterParams.RequestedFilters is not TLookup lookup)
                throw new ArgumentException("Invalid lookup type");

            // The requested filter from the user
            FilterDefinition<TEntity> filter = await _filterBuilder.Build((TLookup)lookup);

            if (filter == null) throw new InvalidOperationException("Failed to build filters from lookup");

            FilterDefinition<TEntity> requiredFilter = _authorisationContentResolver.BuildAffiliatedFilterParams<TEntity>();

            FilterDefinition<TEntity> combinedFilter = Builders<TEntity>.Filter.And(filter, requiredFilter);

            // Count matching documents asynchronously
            long count = await _mongoDbService.GetCollection<TEntity>().CountDocumentsAsync(combinedFilter);

            // Succeed if count > 0, otherwise fail
            if (count > 0) context.Succeed(requirement);
            else context.Fail();
        }
    }

}