using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
using Pawfect_Pet_Adoption_App_API.Services.Convention;
using Pawfect_Pet_Adoption_App_API.Services.FilterServices;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        private readonly IConventionService _conventionService;
        private readonly ClaimsExtractor _claimsExtractor;
        private readonly IFilterBuilder<TEntity, TLookup> _filterBuilder;
        private readonly IAuthorisationContentResolver _authorisationContentResolver;
        private readonly MongoDbService _mongoDbService;

        public AffiliatedRequirementHandler(
            IConventionService conventionService,
            ClaimsExtractor claimsExtractor,
            IFilterBuilder<TEntity, TLookup> filterBuilder,
            IAuthorisationContentResolver authorisationContentResolver,
            MongoDbService mongoDbService)
        {
            _conventionService = conventionService;
            _claimsExtractor = claimsExtractor;
            _filterBuilder = filterBuilder;
            _authorisationContentResolver = authorisationContentResolver;
            _mongoDbService = mongoDbService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AffiliatedRequirement requirement)
        {
            AffiliatedResource affiliatedResource = requirement.Resource;
            if (affiliatedResource == null ||
                !affiliatedResource.UserIds.Any() ||
                !affiliatedResource.AffiliatedRoles.Any() ||
                affiliatedResource.AffiliatedFilterParams == null)
            {
                context.Fail();
                return;
            }

            // Basic affiliation check up
            String userId = _claimsExtractor.CurrentUserId(context.User);
            List<String> userRoles = _claimsExtractor.CurrentUserRoles(context.User) ?? new List<String>();

            Boolean isUserAffiliated = _conventionService.IsValidId(userId) && requirement.Resource.UserIds.Contains(userId);
            Boolean hasAffiliatedRole = requirement.Resource.AffiliatedRoles != null && userRoles.Intersect(requirement.Resource.AffiliatedRoles).Any();

            if (!isUserAffiliated || !hasAffiliatedRole)
            {
                context.Fail();
                return;
            }
            // --

            // * Intenral affiliation lookup *

            // Validate that the lookup type matches TLookup
            if (affiliatedResource.AffiliatedFilterParams.Lookup is not TLookup lookup)
            {
                context.Fail();
                return;
            }

            // Validate that the entity type matches
            Type entityType = affiliatedResource.AffiliatedFilterParams.Lookup.GetEntityType();
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

            FilterDefinition<TEntity> requiredFilter = _authorisationContentResolver.BuildAffiliatedFilterParams<TEntity>();

            FilterDefinition<TEntity> combinedFilter = Builders<TEntity>.Filter.And(filter, requiredFilter);

            // Count matching documents asynchronously
            long count = await _mongoDbService.GetCollection<TEntity>().CountDocumentsAsync(combinedFilter);

            // Succeed if count > 1, otherwise fail
            if (count > 1) context.Succeed(requirement);
            else context.Fail();
        }
    }
}