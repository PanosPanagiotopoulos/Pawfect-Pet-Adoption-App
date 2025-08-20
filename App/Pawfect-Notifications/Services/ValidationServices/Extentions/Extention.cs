using FluentValidation;
using FluentValidation.AspNetCore;
using Pawfect_Notifications.Models.Notification;

namespace Pawfect_Notifications.Services.ValidationServices.Extentions
{
	public static class Extention
	{
		public static IServiceCollection AddValidationServices(this IServiceCollection services)
		{
			// Controllers and FluentValidation
			services.AddControllers()
				.AddFluentValidation(fv => fv.DisableDataAnnotationsValidation = true);

			services.AddValidatorsFromAssemblyContaining<NotificationValidator>();

			return services;
		}
	}



}
