using Microsoft.AspNetCore.Authorization;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authentication;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;

namespace Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices.Extentions
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


            // ** Authorisation * //
            // * Configs *
            services.Configure<PermissionPolicyProviderConfig>(policiesConfigurations);
            
            // * Services *

            // Direct Authorisation And Resolver services
            services.AddScoped<IAuthorisationService, AuthorisationService>();
            services.AddScoped<IAuthorisationContentResolver, AuthorisationContentResolver>();
            // Providers
            services.AddScoped<PermissionPolicyProvider>();
            services.AddScoped<ClaimsExtractor>();


            // * Authorisation Requirement & Handlers*
            // Affiliated:
            services.AddScoped<IAuthorizationHandler, AffiliatedRequirementHandler<Animal, AnimalLookup>>();
            services.AddScoped<IAuthorizationHandler, AffiliatedRequirementHandler<AdoptionApplication, AdoptionApplicationLookup>>();
            services.AddScoped<IAuthorizationHandler, AffiliatedRequirementHandler<Message, MessageLookup>>();
            services.AddScoped<IAuthorizationHandler, AffiliatedRequirementHandler<Conversation, ConversationLookup>>();
            services.AddScoped<IAuthorizationHandler, AffiliatedRequirementHandler<Report, ReportLookup>>();
            services.AddScoped<IAuthorizationHandler, AffiliatedRequirementHandler<Notification, NotificationLookup>>();
            services.AddScoped<IAuthorizationHandler, AffiliatedRequirementHandler<Data.Entities.File, FileLookup>>();
            services.AddScoped<IAuthorizationHandler, AffiliatedRequirementHandler<Shelter, ShelterLookup>>();
            services.AddScoped<IAuthorizationHandler, AffiliatedRequirementHandler<User, UserLookup>>();

            // Owned: 
            services.AddScoped<IAuthorizationHandler, OwnedRequirementHandler<Animal, AnimalLookup>>();
            services.AddScoped<IAuthorizationHandler, OwnedRequirementHandler<AdoptionApplication, AdoptionApplicationLookup>>();
            services.AddScoped<IAuthorizationHandler, OwnedRequirementHandler<Message, MessageLookup>>();
            services.AddScoped<IAuthorizationHandler, OwnedRequirementHandler<Conversation, ConversationLookup>>();
            services.AddScoped<IAuthorizationHandler, OwnedRequirementHandler<Report, ReportLookup>>();
            services.AddScoped<IAuthorizationHandler, OwnedRequirementHandler<Notification, NotificationLookup>>();
            services.AddScoped<IAuthorizationHandler, OwnedRequirementHandler<Data.Entities.File, FileLookup>>();
            services.AddScoped<IAuthorizationHandler, OwnedRequirementHandler<Shelter, ShelterLookup>>();
            services.AddScoped<IAuthorizationHandler, OwnedRequirementHandler<User, UserLookup>>();

			return services;
		}
	}
}
