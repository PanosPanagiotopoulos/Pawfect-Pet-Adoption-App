using FluentValidation;
using FluentValidation.AspNetCore;

using Main_API.Models;
using Main_API.Models.AdoptionApplication;
using Main_API.Models.Animal;
using Main_API.Models.AnimalType;
using Main_API.Models.Breed;
using Main_API.Models.Conversation;
using Main_API.Models.File;
using Main_API.Models.Message;
using Main_API.Models.Notification;
using Main_API.Models.Report;
using Main_API.Models.Shelter;
using Main_API.Models.User;

namespace Main_API.Services.ValidationServices.Extentions
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
