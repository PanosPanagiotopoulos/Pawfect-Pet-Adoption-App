using FluentValidation;
using FluentValidation.AspNetCore;

using Pawfect_Pet_Adoption_App_API.Models;
using Pawfect_Pet_Adoption_App_API.Models.AdoptionApplication;
using Pawfect_Pet_Adoption_App_API.Models.Animal;
using Pawfect_Pet_Adoption_App_API.Models.AnimalType;
using Pawfect_Pet_Adoption_App_API.Models.Breed;
using Pawfect_Pet_Adoption_App_API.Models.Conversation;
using Pawfect_Pet_Adoption_App_API.Models.File;
using Pawfect_Pet_Adoption_App_API.Models.Message;
using Pawfect_Pet_Adoption_App_API.Models.Notification;
using Pawfect_Pet_Adoption_App_API.Models.Report;
using Pawfect_Pet_Adoption_App_API.Models.Shelter;
using Pawfect_Pet_Adoption_App_API.Models.User;

namespace Pawfect_Pet_Adoption_App_API.Services.ValidationServices.Extentions
{
	public static class Extention
	{
		public static IServiceCollection AddValidationServices(this IServiceCollection services)
		{
			// Controllers and FluentValidation
			services.AddControllers()
				.AddFluentValidation(fv => fv.DisableDataAnnotationsValidation = true);

			services.AddValidatorsFromAssemblyContaining<UserValidator>();
			services.AddValidatorsFromAssemblyContaining<ShelterValidator>();
			services.AddValidatorsFromAssemblyContaining<ReportValidator>();
			services.AddValidatorsFromAssemblyContaining<NotificationValidator>();
			services.AddValidatorsFromAssemblyContaining<MessageValidator>();
			services.AddValidatorsFromAssemblyContaining<ConversationValidator>();
			services.AddValidatorsFromAssemblyContaining<BreedValidator>();
			services.AddValidatorsFromAssemblyContaining<AnimalTypeValidator>();
			services.AddValidatorsFromAssemblyContaining<AnimalValidator>();
			services.AddValidatorsFromAssemblyContaining<AdoptionApplicationValidator>();
			services.AddValidatorsFromAssemblyContaining<RegisterValidator>();
			services.AddValidatorsFromAssemblyContaining<FileValidator>();

			return services;
		}
	}



}
