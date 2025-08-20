using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;

namespace Pawfect_API.Services.UserServices.Extentions
{
	public static class Extention
	{
		public static IServiceCollection AddUserServices(this IServiceCollection services, IConfiguration userFields)
		{
			services.AddScoped<IUserService, UserService>();
			services.AddScoped(provider => new Lazy<IUserService>(() => provider.GetRequiredService<IUserService>()));

			services.Configure<UserFields>(userFields);

            return services;
		}
	}
}
