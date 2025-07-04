﻿using MongoDB.Bson;
using MongoDB.Driver;
using Main_API.Data.Entities.Types.Authorization;
using System.Security.Claims;

namespace Main_API.Services.AuthenticationServices
{
    public interface IAuthorizationContentResolver
    {
        ClaimsPrincipal CurrentPrincipal();
        Task<String> CurrentPrincipalShelter();
        List<String> AffiliatedRolesOf(params String[] permissions);
        OwnedResource BuildOwnedResource(Models.Lookups.Lookup requestedFilters, String userId);
        OwnedResource BuildOwnedResource(Models.Lookups.Lookup requestedFilters, List<String> userIds = null);
        AffiliatedResource BuildAffiliatedResource(Models.Lookups.Lookup requestedFilters, List<String> affiliatedRoles = null);
        AffiliatedResource BuildAffiliatedResource(Models.Lookups.Lookup requestedFilters, params String[] permissions);
        Task<FilterDefinition<BsonDocument>> BuildAffiliatedFilterParams(Type entityType);
        FilterDefinition<BsonDocument> BuildOwnedFilterParams(Type entityType);
    }
}
