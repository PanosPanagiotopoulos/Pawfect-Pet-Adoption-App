using Microsoft.AspNetCore.Authorization;
using Pawfect_Notifications.Data.Entities;
using Pawfect_Notifications.Data.Entities.Types.Authentication;
using Pawfect_Notifications.Data.Entities.Types.Authorization;
using Pawfect_Notifications.Data.Entities.Types.Authorisation;

namespace Pawfect_Notifications.Services.AuthenticationServices.Extentions
{
	public static class Extention
	{
		public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, 
																   IConfiguration authenticationConfigurations,
																   IConfiguration policiesConfigurations)
		{
			// Authentication
			services.Configure<JwtConfig>(authenticationConfigurations.GetSection("Jwt"));
            services.Configure<InternalApiConfig>(authenticationConfigurations.GetSection("InternalApi"));

            services.AddSingleton<JwtService>();


            // ** Authorization * //
            // * Configs *
            services.Configure<PermissionPolicyProviderConfig>(policiesConfigurations);
            
            // * Services *

            // Direct Authorization And Resolver services
            services.AddScoped<IAuthorizationService, AuthorizationService>();
            services.AddScoped<IAuthorizationContentResolver, AuthorizationContentResolver>();
            services.AddScoped<AuthContextBuilder>();
            // Providers
            services.AddScoped<PermissionPolicyProvider>();
            services.AddScoped<ClaimsExtractor>();


            // * Authorization Requirement & Handlers*
            // Affiliated:
            services.AddScoped<IAuthorizationHandler, AffiliatedRequirementHandler>();
            // Owned: 
            services.AddScoped<IAuthorizationHandler, OwnedRequirementHandler>();

			return services;
		}
	}
}
