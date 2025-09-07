using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Pawfect_Messenger.Data.Entities;
using Pawfect_Messenger.Data.Entities.Types.Cache;
using Pawfect_Messenger.Exceptions;
using Pawfect_Messenger.Models.Lookups;
using System.Collections.Concurrent;
using System.Security.Claims;
using Pawfect_Messenger.DevTools;
using Pawfect_Messenger.Data.Entities.EnumTypes;
using Pawfect_Messenger.Query;
using Pawfect_Messenger.Services.Convention;
using Pawfect_Messenger.Data.Entities.Types.Authorisation;
using Pawfect_Messenger.Query.Queries;

namespace Pawfect_Messenger.Services.AuthenticationServices
{
    public class AuthorizationContentResolver : IAuthorizationContentResolver
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly PermissionPolicyProvider _permissionPolicyProvider;
        private readonly IConventionService _conventionService;
        private readonly ClaimsExtractor _claimsExtractor;
        private readonly IMemoryCache _memoryCache;
        private readonly CacheConfig _cacheConfig;
        private readonly IQueryFactory _queryFactory;

        public AuthorizationContentResolver(
            IHttpContextAccessor httpContextAccessor,
            PermissionPolicyProvider permissionPolicyProvider,
            IConventionService conventionService,
            ClaimsExtractor claimsExtractor,
            IMemoryCache memoryCache,
            IOptions<CacheConfig> cacheConfig,
            IQueryFactory queryFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _permissionPolicyProvider = permissionPolicyProvider;
            _conventionService = conventionService;
            _claimsExtractor = claimsExtractor;
            _memoryCache = memoryCache;
            _cacheConfig = cacheConfig.Value;
            _queryFactory = queryFactory;
        }

        // Caches for affiliated and owned filters
        private readonly ConcurrentDictionary<(Type EntityType, String UserId), FilterDefinition<BsonDocument>> _affiliatedFilterCache = new();
        private readonly ConcurrentDictionary<(Type EntityType, String UserId), FilterDefinition<BsonDocument>> _ownedFilterCache = new();

        public ClaimsPrincipal CurrentPrincipal() => _httpContextAccessor.HttpContext?.User;

        public List<String> AffiliatedRolesOf(params String[] permissions) => _permissionPolicyProvider.GetAllAffiliatedRolesForPermissions(permissions)?.ToList() ?? new List<String>();
        public async Task<String> CurrentPrincipalShelter()
        {
            String userId = _claimsExtractor.CurrentUserId(CurrentPrincipal());
            if (!_conventionService.IsValidId(userId)) throw new UnAuthenticatedException("No authenticated user found");

            // Check cache
            if (_memoryCache.TryGetValue(userId, out String cachedShelterId)) return cachedShelterId;

            ShelterQuery shelterQuery = _queryFactory.Query<ShelterQuery>();
            shelterQuery.UserIds = new List<String> { userId };
            shelterQuery.Fields = [nameof(Models.Shelter.Shelter.Id)];
            shelterQuery.Offset = 0;
            shelterQuery.PageSize = 1;

            String shelterId = (await shelterQuery.CollectAsync())?.FirstOrDefault()?.Id;
            if (_conventionService.IsValidId(shelterId)) _memoryCache.Set(userId, shelterId, TimeSpan.FromMinutes(_cacheConfig.ShelterDataCacheTime));

            return shelterId;
        }
        public OwnedResource BuildOwnedResource(Models.Lookups.Lookup requestedFilters, String ownerId) => this.BuildOwnedResource(requestedFilters, new List<String>() { ownerId });
        public OwnedResource BuildOwnedResource(Lookup requestedFilters, List<String> ownerIds = null)
        {
            return new OwnedResource(ownerIds ?? new List<String>(), new OwnedFilterParams(requestedFilters));
        }

        public AffiliatedResource BuildAffiliatedResource(Lookup requestedFilters, List<String> affiliatedRoles = null)
        {
            return new AffiliatedResource(affiliatedRoles, new AffiliatedFilterParams(requestedFilters));
        }

        public AffiliatedResource BuildAffiliatedResource(Lookup requestedFilters, params String[] permissions)
        {
            return new AffiliatedResource(AffiliatedRolesOf(permissions), new AffiliatedFilterParams(requestedFilters));
        }

        public async Task<FilterDefinition<BsonDocument>> BuildAffiliatedFilterParams(Type entityType)
        {
            ClaimsPrincipal claimsPrincipal = CurrentPrincipal();
            String userId = _claimsExtractor.CurrentUserId(claimsPrincipal);
            if (!_conventionService.IsValidId(userId)) throw new ForbiddenException("No claims id found");

            String shelterId = await CurrentPrincipalShelter();

            // Check cache
            (Type EntityType, String UserId) cacheKey = (EntityType: entityType, UserId: userId);
            if (_affiliatedFilterCache.TryGetValue(cacheKey, out FilterDefinition<BsonDocument> cachedFilter)) return cachedFilter;

            // Apply filters
            FilterDefinitionBuilder<BsonDocument> bsonBuilder = Builders<BsonDocument>.Filter;
            FilterDefinition<BsonDocument> finalFilter = bsonBuilder.Empty;
            switch (entityType.Name)
            {
                case nameof(Data.Entities.File):
                    {
                        FilterDefinitionBuilder<Data.Entities.File> builder = Builders<Data.Entities.File>.Filter;

                        // Allow access if file is public OR if user is the owner
                        FilterDefinition<Data.Entities.File> filter = builder.Or(
                            builder.Eq(nameof(Data.Entities.File.AccessType), FileAccessType.Public),
                            builder.Eq(nameof(Data.Entities.File.OwnerId), new ObjectId(userId))
                        );

                        if (!String.IsNullOrEmpty(shelterId))
                            filter = builder.Or(filter, builder.Eq(nameof(Data.Entities.File.ContextId), new ObjectId(shelterId)));

                        finalFilter = MongoHelper.ToBsonFilter<Data.Entities.File>(filter);
                        break;
                    }
            }

            // Cache the result
            _affiliatedFilterCache.TryAdd(cacheKey, finalFilter);

            return finalFilter;
        }

        public FilterDefinition<BsonDocument> BuildOwnedFilterParams(Type entityType)
        {
            ClaimsPrincipal claimsPrincipal = CurrentPrincipal();
            String userId = _claimsExtractor.CurrentUserId(claimsPrincipal);
            if (!_conventionService.IsValidId(userId)) throw new ForbiddenException("No claims id found");

            // Check cache
            (Type EntityType, String UserId) cacheKey = (EntityType: entityType, UserId: userId);
            if (_ownedFilterCache.TryGetValue(cacheKey, out FilterDefinition<BsonDocument> cachedFilter)) return cachedFilter;

            FilterDefinitionBuilder<BsonDocument> bsonBuilder = Builders<BsonDocument>.Filter;
            FilterDefinition<BsonDocument> finalFilter = bsonBuilder.Empty;
            switch (entityType.Name)
            {
                case nameof(Data.Entities.File):
                    {
                        FilterDefinitionBuilder<Data.Entities.File> builder = Builders<Data.Entities.File>.Filter;

                        // Allow access if file is public OR if user is the owner
                        FilterDefinition<Data.Entities.File> filter = builder.Or(
                            builder.Eq(nameof(Data.Entities.File.AccessType), FileAccessType.Public),
                            builder.Eq(nameof(Data.Entities.File.OwnerId), new ObjectId(userId))
                        );

                        finalFilter = MongoHelper.ToBsonFilter<Data.Entities.File>(filter);
                        break;
                    }

                case nameof(Data.Entities.User):
                    {
                        FilterDefinitionBuilder<User> builder = Builders<User>.Filter;
                        FilterDefinition<User> filter = builder.Empty;

                        // Filter for File.OwnerId to equal userId
                        filter &= builder.In(nameof(Data.Entities.User.Id), [new ObjectId(userId)]);

                        finalFilter = MongoHelper.ToBsonFilter<User>(filter);
                        break;
                    }
             }

            // Cache the result
            _ownedFilterCache.TryAdd(cacheKey, finalFilter);

            return finalFilter;
        }
    }
}