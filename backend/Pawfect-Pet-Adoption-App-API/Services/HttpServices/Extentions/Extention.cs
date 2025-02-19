namespace Pawfect_Pet_Adoption_App_API.Services.HttpServices.Extentions
{
	public static class Extention
	{
		public static IServiceCollection AddHttpServices(this IServiceCollection services)
		{
			services.AddSingleton<RequestService>();

			return services;
		}
	}
}
