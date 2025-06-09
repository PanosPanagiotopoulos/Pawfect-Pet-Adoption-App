namespace Pawfect_Pet_Adoption_App_API.Services.UserServices.Extentions
{
	public static class Extention
	{
		public static IServiceCollection AddUserServices(this IServiceCollection services)
		{
			services.AddScoped<IUserService, UserService>();
			services.AddScoped(provider => new Lazy<IUserService>(() => provider.GetRequiredService<IUserService>()));


			return services;
		}
	}
}
