using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Main_API.Data.Entities.Types.Authorization;
using Main_API.Data.Entities.Types.Cache;
using Main_API.Exceptions;
using Main_API.Services.Convention;
using Main_API.Services.FilterServices;
using Main_API.Services.MongoServices;

namespace Main_API.Services.AuthenticationServices
{
    public class OwnedRequirement : IAuthorizationRequirement
    {
        public OwnedResource Resource { get; }

        public OwnedRequirement(OwnedResource resource)
        {
            Resource = resource;
        }
    }

    public class OwnedRequirementHandler: AuthorizationHandler<OwnedRequirement>
    {
        private readonly IAuthorizationContentResolver _authorizationContentResolver;
        private readonly IConventionService _conventionService;
        private readonly ClaimsExtractor _claimsExtractor;
        private readonly IFilterBuilder _filterBuilder;
        private readonly IMemoryCache _memoryCache;
        private readonly MongoDbService _mongoDbService;
        private readonly CacheConfig _cacheConfig;

        public OwnedRequirementHandler
        (
            IAuthorizationContentResolver contentResolver,
            IConventionService conventionService,
            ClaimsExtractor claimsExtractor,
            IFilterBuilder filterBuilder,
            IMemoryCache memoryCache,
            IOptions<CacheConfig> cacheConfig,
            MongoDbService mongoDbService)
        {
            _authorizationContentResolver = contentResolver;
            _conventionService = conventionService;
            _claimsExtractor = claimsExtractor;
            _filterBuilder = filterBuilder;
            _memoryCache = memoryCache;
            _cacheConfig = cacheConfig.Value;
            _mongoDbService = mongoDbService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, OwnedRequirement requirement)
        {
            String userId = _claimsExtractor.CurrentUserId(context.User);
            if (!_conventionService.IsValidId(userId)) throw new ForbiddenException("User is not authenticated.");

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

            String cacheKey = $"owner_{userId}_of_{ownedResource.OwnedFilterParams.RequestedFilters.GetHashCode()}";
            if (!_memoryCache.TryGetValue(cacheKey, out long? count))
            {
                count = await this.CalculateOwnedDocuments(ownedResource);
                _memoryCache.Set(cacheKey, count, TimeSpan.FromMinutes(_cacheConfig.RequirementResultTime));
            }

            // Succeed if count > 0, otherwise fail
            if (count > 0) context.Succeed(requirement);
            else context.Fail();
        }

        private async Task<long> CalculateOwnedDocuments(OwnedResource ownedResource)
        {
            // The requested filter from the user
            FilterDefinition<BsonDocument> filter = await _filterBuilder.Build(ownedResource.OwnedFilterParams.RequestedFilters);

            if (filter == null) throw new InvalidOperationException("Failed to build filters from lookup");

            FilterDefinition<BsonDocument> requiredFilter = _authorizationContentResolver.BuildOwnedFilterParams(ownedResource.OwnedFilterParams.RequestedFilters.GetEntityType());

            FilterDefinition<BsonDocument> combinedFilter = Builders<BsonDocument>.Filter.And(filter, requiredFilter);

            // Count matching documents asynchronously
            return await _mongoDbService.GetCollection(ownedResource.OwnedFilterParams.RequestedFilters.GetEntityType()).CountDocumentsAsync(combinedFilter);
        }
    }
}