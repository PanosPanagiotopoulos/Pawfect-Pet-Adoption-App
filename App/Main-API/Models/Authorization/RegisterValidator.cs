using FluentValidation;

using Main_API.Models.Shelter;
using Main_API.Models.User;

namespace Main_API.Models
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
