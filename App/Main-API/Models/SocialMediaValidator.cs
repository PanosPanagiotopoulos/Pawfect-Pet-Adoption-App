using FluentValidation;
using Microsoft.Extensions.Logging;
using Main_API.Data.Entities.HelperModels;
using System.Text.RegularExpressions;

namespace Main_API.Models
{
    public class SocialMediaValidator : AbstractValidator<SocialMedia>
    {

        public SocialMediaValidator()
        {
            When(socialMedia => socialMedia != null, () =>
            {
                // Validate Facebook URL when provided
                When(socialMedia => !String.IsNullOrEmpty(socialMedia.Facebook), () =>
                {
                    RuleFor(socialMedia => socialMedia.Facebook)
                        .Cascade(CascadeMode.Stop)
                        .Must(BeValidFacebookUrl)
                        .WithMessage("Please provide a valid Facebook URL");
                });

                // Validate Instagram URL when provided
                When(socialMedia => !String.IsNullOrEmpty(socialMedia.Instagram), () =>
                {
                    RuleFor(socialMedia => socialMedia.Instagram)
                        .Cascade(CascadeMode.Stop)
                        .Must(BeValidInstagramUrl)
                        .WithMessage("Please provide a valid Instagram URL");
                });
            });
        }

        private Boolean BeValidFacebookUrl(String url)
        {
            if (String.IsNullOrEmpty(url))
                return false;

            // Regex: http(s)://(optional subdomain).facebook.com/anything
            Regex regex = new Regex(@"^https?:\/\/([a-zA-Z0-9-]+\.)*facebook\.com(\/.*)?$", RegexOptions.IgnoreCase);
            Boolean isValid = regex.IsMatch(url);


            return isValid;
        }

        private Boolean BeValidInstagramUrl(String url)
        {
            if (String.IsNullOrEmpty(url)) return false;

            // Regex: http(s)://(optional subdomain).instagram.com/anything
            Regex regex = new Regex(@"^https?:\/\/([a-zA-Z0-9-]+\.)*instagram\.com(\/.*)?$", RegexOptions.IgnoreCase);
            Boolean isValid = regex.IsMatch(url);

            return isValid;
        }
    }
}