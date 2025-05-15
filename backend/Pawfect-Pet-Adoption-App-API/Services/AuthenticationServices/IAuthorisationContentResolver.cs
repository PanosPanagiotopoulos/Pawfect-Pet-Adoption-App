using MongoDB.Driver;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
using System.Security.Claims;

namespace Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices
{
    public interface IAuthorisationContentResolver
    {
        ClaimsPrincipal CurrentPrincipal();
        FilterDefinition<TEntity> BuildAffiliatedFilterParams<TEntity>() where TEntity : class;
        FilterDefinition<TEntity> BuildOwnedFilterParams<TEntity>() where TEntity : class;
    }
}
