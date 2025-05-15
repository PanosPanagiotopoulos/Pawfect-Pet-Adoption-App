using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
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
            // Basic ownership check
            if (!_conventionService.IsValidId(userId) || !requirement.Resource.UserIds.Contains(userId))
            {
                context.Fail();
                return;
            }

            OwnedResource ownedResource = requirement.Resource;
            if (ownedResource == null ||
                ownedResource.OwnedFilterParams == null)
            {
                context.Fail();
                return;
            }

            // * Intenral affiliation lookup *

            // Validate that the lookup type matches TLookup
            if (ownedResource.OwnedFilterParams.Lookup is not TLookup lookup)
            {
                context.Fail();
                return;
            }

            // Validate that the entity type matches
            Type entityType = ownedResource.OwnedFilterParams.Lookup.GetEntityType();
            if (entityType != typeof(TEntity))
            {
                context.Fail();
                return;
            }

            // Build the filter using the strongly typed IFilterBuilder
            FilterDefinition<TEntity> filter = await _filterBuilder.Build(lookup);
            if (filter == null)
            {
                throw new Exception("Failed to build filters from lookup");
            }

            FilterDefinition<TEntity> requiredFilter = _authorisationContentResolver.BuildOwnedFilterParams<TEntity>();

            FilterDefinition<TEntity> combinedFilter = Builders<TEntity>.Filter.And(filter, requiredFilter);

            // Count matching documents asynchronously
            long count = await _mongoDbService.GetCollection<TEntity>().CountDocumentsAsync(combinedFilter);

            // Succeed if count > 1, otherwise fail
            if (count > 1) context.Succeed(requirement);
            else context.Fail();


        }
    }

}