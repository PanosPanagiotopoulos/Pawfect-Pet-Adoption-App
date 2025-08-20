namespace Pawfect_API.Services.MongoServices.Extentions
{
	public static class Extention
	{
		public static IServiceCollection AddMongoServices(this IServiceCollection services)
		{
			services.AddScoped<MongoDbService>();
			services.AddTransient<Seeder>();

			return services;
		}
	}
}
