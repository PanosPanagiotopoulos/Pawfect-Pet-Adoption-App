using FluentValidation;

namespace Pawfect_Pet_Adoption_App_API.Models
{
    public class OTPVerificationValidator : AbstractValidator<OTPVerification>
    {
        public OTPVerificationValidator()
        {
            RuleFor(x => x.Otp)
                .InclusiveBetween(1000, 9999)
                .WithMessage("OTP πρέπει να είναι αριθμος τεσσάρων digit.");
        }
    }
}
