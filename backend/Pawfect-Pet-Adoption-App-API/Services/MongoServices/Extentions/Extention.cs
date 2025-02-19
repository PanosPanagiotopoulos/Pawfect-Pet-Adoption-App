namespace Pawfect_Pet_Adoption_App_API.Services.MongoServices.Extentions
{
	public static class Extention
	{
		public static IServiceCollection AddMongoServices(this IServiceCollection services)
		{
			services.AddSingleton<MongoDbService>();
			services.AddTransient<Seeder>();

			return services;
		}
	}
}
