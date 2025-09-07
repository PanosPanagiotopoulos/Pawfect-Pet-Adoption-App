using FluentValidation;
using FluentValidation.AspNetCore;
using Pawfect_Messenger.Models.Conversation;
using Pawfect_Messenger.Models.Message;


namespace Pawfect_Messenger.Services.ValidationServices.Extentions
{
	public static class Extention
	{
		public static IServiceCollection AddValidationServices(this IServiceCollection services)
		{
			// Controllers and FluentValidation
			services.AddControllers()
				.AddFluentValidation(fv => fv.DisableDataAnnotationsValidation = true);

			services.AddValidatorsFromAssemblyContaining<MessageValidator>();
			services.AddValidatorsFromAssemblyContaining<ConversationValidator>();

			return services;
		}
	}



}
