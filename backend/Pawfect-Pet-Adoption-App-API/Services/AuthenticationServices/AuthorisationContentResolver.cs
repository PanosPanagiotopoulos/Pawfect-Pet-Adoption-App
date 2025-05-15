using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Cache;
using Pawfect_Pet_Adoption_App_API.Services.Convention;
using Pawfect_Pet_Adoption_App_API.Services.MongoServices;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices
{
    public class AuthorisationContentResolver : IAuthorisationContentResolver
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConventionService _conventionService;
        private readonly ClaimsExtractor _claimsExtractor;

        public AuthorisationContentResolver(
            IHttpContextAccessor httpContextAccessor,
            IConventionService conventionService,
            ClaimsExtractor claimsExtractor)
        {
            _httpContextAccessor = httpContextAccessor;
            _conventionService = conventionService;
            _claimsExtractor = claimsExtractor;
        }

        // Caches for affiliated and owned filters
        private readonly ConcurrentDictionary<(Type EntityType, String UserId), object> _affiliatedFilterCache = new();
        private readonly ConcurrentDictionary<(Type EntityType, String UserId), object> _ownedFilterCache = new();

        public ClaimsPrincipal CurrentPrincipal() => _httpContextAccessor.HttpContext?.User ?? throw new InvalidOperationException("No current user context available.");

        public FilterDefinition<TEntity> BuildAffiliatedFilterParams<TEntity>() where TEntity: class
        {
            ClaimsPrincipal claimsPrincipal = this.CurrentPrincipal();
            String userId = _claimsExtractor.CurrentUserId(claimsPrincipal);
            if (!_conventionService.IsValidId(userId)) throw new Exception("No claims id found");


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
                        filter = builder.Eq(nameof(AdoptionApplication.UserId), userId);
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
            if (!_conventionService.IsValidId(userId)) throw new Exception("No claims id found");

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
            }

            // Cache the result
            _ownedFilterCache.TryAdd(cacheKey, filter);

            return filter;
        }
    }
}