namespace Pawfect_Pet_Adoption_App_API.Services.SearchServices.Extentions
{
	public static class Extention
	{
		public static IServiceCollection AddSearchServices(this IServiceCollection services)
		{
			services.AddScoped<SearchService>();
			// HttpClient
			services.AddHttpClient<SearchService>(client =>
			{
				client.BaseAddress = new Uri("http://localhost:5000");
			});
			return services;
		}
	}
}
