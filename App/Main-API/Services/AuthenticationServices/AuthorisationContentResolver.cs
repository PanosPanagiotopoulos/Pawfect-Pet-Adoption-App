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
        private readonly ConcurrentDictionary<(Type EntityType, String UserId), object> _affiliatedFilterCache = new();
        private readonly ConcurrentDictionary<(Type EntityType, String UserId), object> _ownedFilterCache = new();

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
            if (_affiliatedFilterCache.TryGetValue(cacheKey, out object cachedFilter)) return (FilterDefinition<BsonDocument>)cachedFilter;

            // Apply filters
            FilterDefinitionBuilder<BsonDocument> builder = Builders<BsonDocument>.Filter;
            FilterDefinition<BsonDocument> filter = builder.Empty;
            switch (entityType.Name)
            {
                case nameof(AdoptionApplication):
                    {
                        filter = builder.Eq(nameof(AdoptionApplication.UserId), new ObjectId(userId));

                        if (!String.IsNullOrEmpty(shelterId))
                        {
                            filter = builder.Or(
                                filter,
                                builder.Eq(nameof(AdoptionApplication.ShelterId), new ObjectId(shelterId))
                            );
                        }

                        break;
                    }

                case nameof(Report):
                    {
                        // Filter for userId to match Report.ReportedId OR Report.ReporterId
                        filter = builder.Or(
                            builder.Eq(nameof(Report.ReportedId), new ObjectId(userId)),
                            builder.Eq(nameof(Report.ReporterId), new ObjectId(userId))
                        );
                        break;
                    }

                case nameof(Message):
                    {
                        // Filter for userId to match Message.SenderId OR Message.RecipientId
                        filter = builder.Or(
                            builder.Eq(nameof(Message.SenderId), new ObjectId(userId)),
                            builder.Eq(nameof(Message.RecipientId), new ObjectId(userId))
                        );

                        break;
                    }

                case nameof(Conversation):
                    {
                        // Filter for userid to be contained in Conversation.UserIds
                        filter = builder.In(nameof(Conversation.UserIds), new[] { new ObjectId(userId) });
                        break;
                    }
            }

            // Cache the result
            _affiliatedFilterCache.TryAdd(cacheKey, filter);

            return filter;
        }

        public FilterDefinition<BsonDocument> BuildOwnedFilterParams(Type entityType)
        {
            ClaimsPrincipal claimsPrincipal = this.CurrentPrincipal();
            String userId = _claimsExtractor.CurrentUserId(claimsPrincipal);
            if (!_conventionService.IsValidId(userId)) throw new ForbiddenException("No claims id found");

            // Check cache
            (Type EntityType, String UserId) cacheKey = (EntityType: entityType, UserId: userId);
            if (_ownedFilterCache.TryGetValue(cacheKey, out object cachedFilter)) return (FilterDefinition<BsonDocument>)cachedFilter;

            FilterDefinitionBuilder<BsonDocument> builder = Builders<BsonDocument>.Filter;
            FilterDefinition<BsonDocument> filter = builder.Empty;

            switch (entityType.Name)
            {
                case nameof(AdoptionApplication):
                    {
                        // Filter for AdoptionApplication.UserId to equal userId
                        filter = builder.Eq(nameof(AdoptionApplication.UserId), new ObjectId(userId));
                        break;
                    }

                case nameof(Notification):
                    {
                        // Filter for Notification.UserId to equal userId
                        filter = builder.Eq(nameof(Notification.UserId), new ObjectId(userId));
                        break;
                    }

                case nameof(Data.Entities.File):
                    {
                        // Filter for File.OwnerId to equal userId
                        filter = builder.Eq(nameof(Data.Entities.File.OwnerId), new ObjectId(userId));
                        break;
                    }

                case nameof(Report):
                    {
                        // Filter for userId to match Report.ReportedId OR Report.ReporterId
                        filter = builder.Eq(nameof(Data.Entities.Report.ReporterId), new ObjectId(userId));
                        break;
                    }

                case nameof(Data.Entities.Message):
                    {
                        // Filter for File.OwnerId to equal userId
                        filter = builder.Eq(nameof(Data.Entities.Message.SenderId), new ObjectId(userId));
                        break;
                    }

                case nameof(Data.Entities.User):
                    {
                        // Filter for File.OwnerId to equal userId
                        filter = builder.Eq(nameof(Data.Entities.User.Id), new ObjectId(userId));
                        break;
                    }
             }

            // Cache the result
            _ownedFilterCache.TryAdd(cacheKey, filter);

            return filter;
        }
    }
}