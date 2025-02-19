namespace Pawfect_Pet_Adoption_App_API.Services.SmsServices.Extentions
{
	public static class Extention
	{
		public static IServiceCollection AddSmsServices(this IServiceCollection services)
		{
			services.AddScoped<ISmsService, SmsService>();

			return services;
		}
	}
}
