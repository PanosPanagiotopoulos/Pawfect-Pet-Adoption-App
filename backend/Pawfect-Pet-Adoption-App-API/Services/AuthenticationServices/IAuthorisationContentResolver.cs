using MongoDB.Driver;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
using System.Security.Claims;

namespace Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices
{
    public interface IAuthorisationContentResolver
    {
        ClaimsPrincipal CurrentPrincipal();
        Task<String> CurrentPrincipalShelter();
        List<String> AffiliatedRolesOf(params String[] permissions);
        OwnedResource BuildOwnedResource(Models.Lookups.Lookup requestedFilters, String userId);
        OwnedResource BuildOwnedResource(Models.Lookups.Lookup requestedFilters, List<String> userIds = null);
        AffiliatedResource BuildAffiliatedResource(Models.Lookups.Lookup requestedFilters, List<String> affiliatedRoles = null);
        AffiliatedResource BuildAffiliatedResource(Models.Lookups.Lookup requestedFilters, params String[] permissions);
        AffiliatedResource BuildAffiliatedResource(Models.Lookups.Lookup requestedFilters, String affiliatedId, params String[] permissions);
        FilterDefinition<TEntity> BuildAffiliatedFilterParams<TEntity>(String affiliatedId = null) where TEntity : class;
        FilterDefinition<TEntity> BuildOwnedFilterParams<TEntity>() where TEntity : class;
    }
}
