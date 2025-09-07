using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Extensions.Options;
using Pawfect_Messenger.Data.Entities.Types.Cache;
using Microsoft.Extensions.Caching.Memory;
using Pawfect_Messenger.Exceptions;
using Pawfect_Messenger.Services.Convention;
using Pawfect_Messenger.Services.FilterServices;
using Pawfect_Messenger.Services.MongoServices;
using Pawfect_Messenger.Data.Entities.Types.Authorisation;

namespace Pawfect_Messenger.Services.AuthenticationServices
{
    public class AffiliatedRequirement : IAuthorizationRequirement
    {
        public AffiliatedResource Resource { get; }

        public AffiliatedRequirement(AffiliatedResource resource)
        {
            Resource = resource;
        }
    }

    public class AffiliatedRequirementHandler : AuthorizationHandler<AffiliatedRequirement>
    {
        private readonly ClaimsExtractor _claimsExtractor;
        private readonly IFilterBuilder _filterBuilder;
        private readonly IAuthorizationContentResolver _authorizationContentResolver;
        private readonly IMemoryCache _memoryCache;
        private readonly IConventionService _conventionService;
        private readonly CacheConfig _cacheConfig;
        private readonly MongoDbService _mongoDbService;

        public AffiliatedRequirementHandler(
            ClaimsExtractor claimsExtractor,
            IFilterBuilder filterBuilder, 
            IAuthorizationContentResolver AuthorizationContentResolver,
            IOptions<CacheConfig> cacheConfig,
            IMemoryCache memoryCache,
            IConventionService conventionService,
            MongoDbService mongoDbService)
        {
            _claimsExtractor = claimsExtractor;
            _filterBuilder = filterBuilder;
            _authorizationContentResolver = AuthorizationContentResolver;
            _memoryCache = memoryCache;
            _conventionService = conventionService;
            _cacheConfig = cacheConfig.Value;
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

            String userId = _claimsExtractor.CurrentUserId(context.User);
            if (!_conventionService.IsValidId(userId)) throw new ForbiddenException("User is not authenticated.");

            String cacheKey = $"affiliated_{userId}_with_{affiliatedResource.AffiliatedFilterParams.RequestedFilters.GetHashCode()}";
            if (!_memoryCache.TryGetValue(cacheKey, out long? count))
            {
                count = await CalculateAffiliatedDocuments(affiliatedResource);
                _memoryCache.Set(cacheKey, count, TimeSpan.FromMinutes(_cacheConfig.RequirementResultTime));
            }

            // Succeed if count > 0, otherwise fail
            if (count > 0) context.Succeed(requirement);
            else context.Fail();
        }

        private async Task<long> CalculateAffiliatedDocuments(AffiliatedResource resource)
        {
            // * Intenral affiliation lookup *

            // The requested filter from the user
            FilterDefinition<BsonDocument> filter = await _filterBuilder.Build(resource.AffiliatedFilterParams.RequestedFilters);

            if (filter == null) throw new InvalidOperationException("Failed to build filters from lookup");

            FilterDefinition<BsonDocument> requiredFilter = await _authorizationContentResolver.BuildAffiliatedFilterParams(resource.AffiliatedFilterParams.RequestedFilters.GetEntityType());

            FilterDefinition<BsonDocument> combinedFilter = Builders<BsonDocument>.Filter.And(filter, requiredFilter);

            // Count matching documents asynchronously
            return await _mongoDbService.GetCollection(resource.AffiliatedFilterParams.RequestedFilters.GetEntityType()).CountDocumentsAsync(combinedFilter);
        }
    }
}