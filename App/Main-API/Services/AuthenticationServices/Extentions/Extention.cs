using Microsoft.AspNetCore.Authorization;
using Main_API.Data.Entities;
using Main_API.Data.Entities.Types.Authentication;
using Main_API.Data.Entities.Types.Authorization;
using Main_API.Models.Lookups;

namespace Main_API.Services.AuthenticationServices.Extentions
{
	public static class Extention
	{
		public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, 
																   IConfiguration authenticationConfigurations,
																   IConfiguration policiesConfigurations)
		{
			// Authentication
			services.Configure<JwtConfig>(authenticationConfigurations.GetSection("Jwt"));
			services.Configure<GoogleOauth2Config>(authenticationConfigurations.GetSection("Google"));

			services.AddScoped<IAuthenticationService, AuthenticationService>();
          
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
