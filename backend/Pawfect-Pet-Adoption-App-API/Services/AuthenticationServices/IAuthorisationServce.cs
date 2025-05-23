﻿using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
using System.Security.Claims;

namespace Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices
{
    public interface IAuthorisationService
    {
        Task<Boolean> AuthorizeAsync(params String[] permissions);
        Task<Boolean> AuthorizeOwnedAsync(OwnedResource resource);
        Task<Boolean> AuthorizeAffiliatedAsync(AffiliatedResource resource);

        Task<Boolean> AuthorizeOrAffiliatedAsync(AffiliatedResource resource, params String[] permissions);

        Task<Boolean> AuthorizeOrOwnedAsync(OwnedResource resource, params String[] permissions);

        Task<Boolean> AuthorizeOrOwnedOrAffiliated(AuthContext context, params String[] permissions);
        Task<Boolean> AuthorizeOrOwnedOrAffiliated(
            OwnedResource ownedResource,
            AffiliatedResource affiliatedResource,
            params String[] permissions);
    }
}