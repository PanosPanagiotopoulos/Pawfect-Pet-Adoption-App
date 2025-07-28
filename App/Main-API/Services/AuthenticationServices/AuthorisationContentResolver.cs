using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Main_API.Data.Entities;
using Main_API.Data.Entities.Types.Authorization;
using Main_API.Data.Entities.Types.Cache;
using Main_API.Exceptions;
using Main_API.Models.Lookups;
using Main_API.Query;
using Main_API.Query.Queries;
using Main_API.Services.Convention;
using System.Collections.Concurrent;
using System.Security.Claims;
using Main_API.DevTools;

namespace Main_API.Services.AuthenticationServices
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
            shelterQuery.Offset = 1;
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
            return new AffiliatedResource(this.AffiliatedRolesOf(permissions), new AffiliatedFilterParams(requestedFilters));
        }

        public async Task<FilterDefinition<BsonDocument>> BuildAffiliatedFilterParams(Type entityType)
        {
            ClaimsPrincipal claimsPrincipal = this.CurrentPrincipal();
            String userId = _claimsExtractor.CurrentUserId(claimsPrincipal);
            if (!_conventionService.IsValidId(userId)) throw new ForbiddenException("No claims id found");

            String shelterId = await this.CurrentPrincipalShelter();

            // Check cache
            (Type EntityType, String UserId) cacheKey = (EntityType: entityType, UserId: userId);
            if (_affiliatedFilterCache.TryGetValue(cacheKey, out FilterDefinition<BsonDocument> cachedFilter)) return cachedFilter;

            // Apply filters
            FilterDefinitionBuilder<BsonDocument> bsonBuilder = Builders<BsonDocument>.Filter;
            FilterDefinition<BsonDocument> finalFilter = bsonBuilder.Empty;
            switch (entityType.Name)
            {
                case nameof(AdoptionApplication):
                    {
                        FilterDefinitionBuilder<AdoptionApplication> builder = Builders<AdoptionApplication>.Filter;
                        FilterDefinition<AdoptionApplication> filter = builder.Empty;

                        filter |= builder.In(nameof(AdoptionApplication.UserId), [new ObjectId(userId)]);

                        if (!String.IsNullOrEmpty(shelterId))
                        {
                            filter |= builder.In(nameof(AdoptionApplication.ShelterId), [new ObjectId(shelterId)]);
                        }

                        finalFilter = MongoHelper.ToBsonFilter<AdoptionApplication>(filter);

                        break;
                    }

                case nameof(Report):
                    {
                        FilterDefinitionBuilder<Report> builder = Builders<Report>.Filter;
                        FilterDefinition<Report> filter = builder.Empty;

                        // Filter for userId to match Report.ReportedId OR Report.ReporterId
                        filter &= builder.Or(
                            builder.In(nameof(Report.ReportedId), [new ObjectId(userId)]),
                            builder.In(nameof(Report.ReporterId), [new ObjectId(userId)])
                        );

                        finalFilter = MongoHelper.ToBsonFilter<Report>(filter);

                        break;
                    }
            }

            // Cache the result
            _affiliatedFilterCache.TryAdd(cacheKey, finalFilter);

            return finalFilter;
        }

        public FilterDefinition<BsonDocument> BuildOwnedFilterParams(Type entityType)
        {
            ClaimsPrincipal claimsPrincipal = this.CurrentPrincipal();
            String userId = _claimsExtractor.CurrentUserId(claimsPrincipal);
            if (!_conventionService.IsValidId(userId)) throw new ForbiddenException("No claims id found");

            // Check cache
            (Type EntityType, String UserId) cacheKey = (EntityType: entityType, UserId: userId);
            if (_ownedFilterCache.TryGetValue(cacheKey, out FilterDefinition<BsonDocument> cachedFilter)) return cachedFilter;

            FilterDefinitionBuilder<BsonDocument> bsonBuilder = Builders<BsonDocument>.Filter;
            FilterDefinition<BsonDocument> finalFilter = bsonBuilder.Empty;
            switch (entityType.Name)
            {
                case nameof(AdoptionApplication):
                    {
                        FilterDefinitionBuilder<AdoptionApplication> builder = Builders<AdoptionApplication>.Filter;
                        FilterDefinition<AdoptionApplication> filter = builder.Empty;

                        // Filter for AdoptionApplication.UserId to equal userId
                        filter &= builder.In(nameof(AdoptionApplication.UserId), [new ObjectId(userId)]);

                        finalFilter = MongoHelper.ToBsonFilter<AdoptionApplication>(filter);
                        break;
                    }

                case nameof(Notification):
                    {
                        FilterDefinitionBuilder<Notification> builder = Builders<Notification>.Filter;
                        FilterDefinition<Notification> filter = builder.Empty;

                        // Filter for Notification.UserId to equal userId
                        filter &= builder.In(nameof(Notification.UserId), [new ObjectId(userId)]);

                        finalFilter = MongoHelper.ToBsonFilter<Notification>(filter);
                        break;
                    }

                case nameof(Data.Entities.File):
                    {
                        FilterDefinitionBuilder<Data.Entities.File> builder = Builders<Data.Entities.File>.Filter;
                        FilterDefinition<Data.Entities.File> filter = builder.Empty;

                        // Filter for File.OwnerId to equal userId
                        filter &= builder.In(nameof(Data.Entities.File.OwnerId), [new ObjectId(userId)]);

                        finalFilter = MongoHelper.ToBsonFilter<Data.Entities.File>(filter);

                        break;
                    }

                case nameof(Report):
                    {
                        FilterDefinitionBuilder<Report> builder = Builders<Report>.Filter;
                        FilterDefinition<Report> filter = builder.Empty;

                        // Filter for userId to match Report.ReportedId OR Report.ReporterId
                        filter &= builder.In(nameof(Data.Entities.Report.ReporterId), [new ObjectId(userId)]);

                        finalFilter = MongoHelper.ToBsonFilter<Report>(filter);
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