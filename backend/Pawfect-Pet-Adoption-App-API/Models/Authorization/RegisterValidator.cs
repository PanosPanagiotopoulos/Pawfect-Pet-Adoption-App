using FluentValidation;

using Pawfect_Pet_Adoption_App_API.Models.Shelter;
using Pawfect_Pet_Adoption_App_API.Models.User;

namespace Pawfect_Pet_Adoption_App_API.Models
{
	public class RegisterValidator : AbstractValidator<RegisterPersist>
	{
		public RegisterValidator()
		{
			RuleFor(x => x.User)
			.SetValidator(new UserValidator());

			When(x => x.Shelter != null, () =>
			{
				RuleFor(x => x.Shelter)
				.SetValidator(new ShelterValidator());
			});
		}
	}
}
