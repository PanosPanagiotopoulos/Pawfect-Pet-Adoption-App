namespace Main_API.Services.ConversationServices.Extention
{
	public static class Extention
	{
		public static IServiceCollection AddConversationServices(this IServiceCollection services)
		{
			services.AddScoped<IConversationService, ConversationService>();
			services.AddScoped(provider => new Lazy<IConversationService>(() => provider.GetRequiredService<IConversationService>()));


			return services;
		}
	}
}
