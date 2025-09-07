using Pawfect_Messenger.Services.MessageServices;

namespace Pawfect_Messenger.Services.MessageServices.Extentions
{
	public static class Extention
	{
		public static IServiceCollection AddMessageServices(this IServiceCollection services)
		{
			services.AddScoped<IMessageService, MessageService>();
			services.AddScoped(provider => new Lazy<IMessageService>(() => provider.GetRequiredService<IMessageService>()));

			return services;
		}
	}
}
