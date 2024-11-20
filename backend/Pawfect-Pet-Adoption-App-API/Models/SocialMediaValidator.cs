using FluentValidation;
using Pawfect_Pet_Adoption_App_API.Data.Entities.HelperModels;

namespace Pawfect_Pet_Adoption_App_API.Models
{
    public class SocialMediaValidator : AbstractValidator<SocialMedia>
    {
        public SocialMediaValidator()
        {
            When(socialMediaData => socialMediaData != null, () =>
            {
                // Όταν τα δεδομένα των κοινωνικών μέσων υπάρχουν
                When(socialMedia => !string.IsNullOrEmpty(socialMedia.Facebook), () =>
                {
                    // Όταν το πεδίο Facebook δεν είναι κενό
                    RuleFor(socialMedia => socialMedia.Facebook)
                    .Cascade(CascadeMode.Stop)
                    .Matches(@"^https?:\/\/(www\.)?facebook\.com\/[A-Za-z0-9._%-]+$")
                    .WithMessage("Παρακαλώ εισάγετε έγκυρο link Facebook");
                });

                When(socialMedia => !string.IsNullOrEmpty(socialMedia.Instagram), () =>
                {
                    // Όταν το πεδίο Instagram δεν είναι κενό
                    RuleFor(socialMedia => socialMedia.Instagram)
                    .Cascade(CascadeMode.Stop)
                    .Matches(@"^https?:\/\/(www\.)?instagram\.com\/[A-Za-z0-9._%-]+$")
                    .WithMessage("Παρακαλώ εισάγετε έγκυρο link Instagram");
                });
            });
        }
    }
}
