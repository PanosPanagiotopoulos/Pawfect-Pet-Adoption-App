namespace Pawfect_Messenger.Services.MongoServices.Extentions
{
	public static class Extention
	{
		public static IServiceCollection AddMongoServices(this IServiceCollection services)
		{
			services.AddScoped<MongoDbService>();

			return services;
		}
	}
}
