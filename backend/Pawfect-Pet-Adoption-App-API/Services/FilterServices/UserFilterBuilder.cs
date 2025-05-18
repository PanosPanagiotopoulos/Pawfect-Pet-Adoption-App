using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Cache;
using Pawfect_Pet_Adoption_App_API.Exceptions;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Query;
using Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices;
using System.Security.Claims;

namespace Pawfect_Pet_Adoption_App_API.Services.FilterServices
{
    public class UserFilterBuilder : IFilterBuilder<User, UserLookup>
    {
        private readonly IQueryFactory _queryFactory;
        private readonly ClaimsExtractor _claimsExtractor;
        private readonly IAuthorisationContentResolver _authorisationContentResolver;
        private readonly IMemoryCache _memoryCache;
        private readonly CacheConfig _cacheConfig;

        public UserFilterBuilder
        (
            IQueryFactory queryFactory,
            ClaimsExtractor claimsExtractor,
            IAuthorisationContentResolver authorisationContentResolver,
            IMemoryCache memoryCache,
            IOptions<CacheConfig> cacheConfig
        )
        {
            this._queryFactory = queryFactory;
            this._claimsExtractor = claimsExtractor;
            this._authorisationContentResolver = authorisationContentResolver;
            this._memoryCache = memoryCache;
            this._cacheConfig = cacheConfig.Value;
        }

        public async Task<FilterDefinition<User>> Build(UserLookup lookup)
        {
            ClaimsPrincipal claimsPrincipal = _authorisationContentResolver.CurrentPrincipal();
            String userId = _claimsExtractor.CurrentUserId(claimsPrincipal);
            if (String.IsNullOrEmpty(userId)) throw new ForbiddenException("User is not authenticated.");
            // Define cache key
            String cacheKey = $"{typeof(User).FullName}_{userId}_{lookup.GetHashCode()}";

            if (!_memoryCache.TryGetValue(cacheKey, out FilterDefinition<User> cachedFilter)) return cachedFilter;

            FilterDefinitionBuilder<User> builder = Builders<User>.Filter;
            FilterDefinition<User> filter = builder.Empty;

            FilterDefinition<User> requestedFilters = await lookup.EnrichLookup(_queryFactory).ApplyFilters();

            // Cache the requested filter 
            _memoryCache.Set(cacheKey, requestedFilters, TimeSpan.FromMinutes(_cacheConfig.RequirementResultTime));

            return requestedFilters;
        }
    }
}
