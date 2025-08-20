using FluentValidation;
using FluentValidation.AspNetCore;

using Pawfect_API.Models;
using Pawfect_API.Models.AdoptionApplication;
using Pawfect_API.Models.Animal;
using Pawfect_API.Models.AnimalType;
using Pawfect_API.Models.Breed;
using Pawfect_API.Models.Conversation;
using Pawfect_API.Models.File;
using Pawfect_API.Models.Message;
using Pawfect_API.Models.Notification;
using Pawfect_API.Models.Report;
using Pawfect_API.Models.Shelter;
using Pawfect_API.Models.User;
using Pawfect_Pet_Adoption_App_API.Models.User;

namespace Pawfect_API.Services.ValidationServices.Extentions
{
	public static class Extention
	{
		public static IServiceCollection AddValidationServices(this IServiceCollection services)
		{
			// Controllers and FluentValidation
			services.AddControllers()
				.AddFluentValidation(fv => fv.DisableDataAnnotationsValidation = true);

			services.AddValidatorsFromAssemblyContaining<UserValidator>();
            services.AddValidatorsFromAssemblyContaining<UserUpdateValidator>();
            services.AddValidatorsFromAssemblyContaining<ShelterValidator>();
			services.AddValidatorsFromAssemblyContaining<ReportValidator>();
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
