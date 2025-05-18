using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Cache;
using Pawfect_Pet_Adoption_App_API.Exceptions;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Query;
using Pawfect_Pet_Adoption_App_API.Query.Queries;
using Pawfect_Pet_Adoption_App_API.Services.Convention;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices
{
    public class AuthorisationContentResolver : IAuthorisationContentResolver
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly PermissionPolicyProvider _permissionPolicyProvider;
        private readonly IConventionService _conventionService;
        private readonly ClaimsExtractor _claimsExtractor;
        private readonly IMemoryCache _memoryCache;
        private readonly CacheConfig _cacheConfig;
        private readonly IQueryFactory _queryFactory;

        public AuthorisationContentResolver(
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

        public ClaimsPrincipal CurrentPrincipal() => _httpContextAccessor.HttpContext?.User ?? throw new UnAuthenticatedException("No current user context available.");

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
        public OwnedResource BuildOwnedResource(Models.Lookups.Lookup requestedFilters, String userId) => this.BuildOwnedResource(requestedFilters, new List<String>() { userId });
        public OwnedResource BuildOwnedResource(Lookup requestedFilters, List<String> userIds = null)
        {
            return new OwnedResource(userIds ?? new List<String>(), new OwnedFilterParams(requestedFilters));
        }

        public AffiliatedResource BuildAffiliatedResource(Lookup requestedFilters, List<String> affiliatedRoles = null)
        {
            return new AffiliatedResource(affiliatedRoles, new AffiliatedFilterParams(requestedFilters));
        }

        public AffiliatedResource BuildAffiliatedResource(Lookup requestedFilters, params String[] permissions)
        {
            return new AffiliatedResource(this.AffiliatedRolesOf(permissions), new AffiliatedFilterParams(requestedFilters));
        }

        public AffiliatedResource BuildAffiliatedResource(Models.Lookups.Lookup requestedFilters, String affiliatedId, params String[] permissions)
        {
            return new AffiliatedResource(this.AffiliatedRolesOf(permissions), new AffiliatedFilterParams(requestedFilters), affiliatedId);
        }

        public FilterDefinition<TEntity> BuildAffiliatedFilterParams<TEntity>(String affiliatedId = null) where TEntity: class
        {
            ClaimsPrincipal claimsPrincipal = this.CurrentPrincipal();
            String userId = _claimsExtractor.CurrentUserId(claimsPrincipal);
            if (!_conventionService.IsValidId(userId)) throw new UnAuthenticatedException("No claims id found");


            // Check cache
            (Type EntityType, String UserId) cacheKey = (EntityType: typeof(TEntity), UserId: userId);
            if (_affiliatedFilterCache.TryGetValue(cacheKey, out object cachedFilter)) return (FilterDefinition<TEntity>)cachedFilter;

            // Apply filters
            FilterDefinitionBuilder<TEntity> builder = Builders<TEntity>.Filter;
            FilterDefinition<TEntity> filter = builder.Empty;
            switch (typeof(TEntity).Name)
            {
                case nameof(AdoptionApplication):
                    {
                        filter = builder.Or(
                            builder.Eq(nameof(AdoptionApplication.UserId), userId),
                            builder.Eq(nameof(AdoptionApplication.ShelterId), affiliatedId)
                        );

                        break;
                    }

                case nameof(Report):
                    {
                        // Filter for userId to match Report.ReportedId OR Report.ReporterId
                        filter = builder.Or(
                            builder.Eq(nameof(Report.ReportedId), userId),
                            builder.Eq(nameof(Report.ReporterId), userId)
                        );
                        break;
                    }

                case nameof(Message):
                    {
                        // Filter for userId to match Message.SenderId OR Message.RecipientId
                        filter = builder.Or(
                            builder.Eq(nameof(Message.SenderId), userId),
                            builder.Eq(nameof(Message.RecipientId), userId)
                        );

                        builder.Eq(nameof(Message.ConversationId), affiliatedId);

                        break;
                    }

                case nameof(Conversation):
                    {
                        // Filter for userid to be contained in Conversation.UserIds
                        filter = builder.In(nameof(Conversation.UserIds), new[] { userId });
                        break;
                    }
            }

            // Cache the result
            _affiliatedFilterCache.TryAdd(cacheKey, filter);

            return filter;
        }

        public FilterDefinition<TEntity> BuildOwnedFilterParams<TEntity>() where TEntity : class
        {
            ClaimsPrincipal claimsPrincipal = this.CurrentPrincipal();
            String userId = _claimsExtractor.CurrentUserId(claimsPrincipal);
            if (!_conventionService.IsValidId(userId)) throw new UnAuthenticatedException("No claims id found");

            // Check cache
            (Type EntityType, String UserId) cacheKey = (EntityType: typeof(TEntity), UserId: userId);
            if (_ownedFilterCache.TryGetValue(cacheKey, out object cachedFilter)) return (FilterDefinition<TEntity>)cachedFilter;

            FilterDefinitionBuilder<TEntity> builder = Builders<TEntity>.Filter;
            FilterDefinition<TEntity> filter = builder.Empty;

            switch (typeof(TEntity).Name)
            {
                case nameof(AdoptionApplication):
                    {
                        // Filter for AdoptionApplication.UserId to equal userId
                        filter = builder.Eq(nameof(AdoptionApplication.UserId), userId);
                        break;
                    }

                case nameof(Notification):
                    {
                        // Filter for Notification.UserId to equal userId
                        filter = builder.Eq(nameof(Notification.UserId), userId);
                        break;
                    }

                case nameof(Data.Entities.File):
                    {
                        // Filter for File.OwnerId to equal userId
                        filter = builder.Eq(nameof(Data.Entities.File.OwnerId), userId);
                        break;
                    }

                case nameof(Report):
                    {
                        // Filter for userId to match Report.ReportedId OR Report.ReporterId
                        filter = builder.Eq(nameof(Data.Entities.Report.ReporterId), userId);
                        break;
                    }

                case nameof(Data.Entities.Message):
                    {
                        // Filter for File.OwnerId to equal userId
                        filter = builder.Eq(nameof(Data.Entities.Message.SenderId), userId);
                        break;
                    }
            }

            // Cache the result
            _ownedFilterCache.TryAdd(cacheKey, filter);

            return filter;
        }
    }
}