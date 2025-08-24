using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
using Pawfect_Pet_Adoption_App_API.Services.UserServices;

namespace Pawfect_API.Services.UserServices.Extentions
{
	public static class Extention
	{
		public static IServiceCollection AddUserServices(this IServiceCollection services, IConfiguration userFields)
		{
			services.AddScoped<IUserService, UserService>();
			services.AddScoped(provider => new Lazy<IUserService>(() => provider.GetRequiredService<IUserService>()));

            services.AddScoped<IUserAvailabilityService, UserAvailabilityService>();
            services.AddScoped(provider => new Lazy<IUserAvailabilityService>(() => provider.GetRequiredService<IUserAvailabilityService>()));

            services.Configure<UserFields>(userFields);

            return services;
		}
	}
}
