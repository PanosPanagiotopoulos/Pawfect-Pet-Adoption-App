using FluentValidation;
using Main_API.DevTools;

namespace Main_API.Models.Notification
{
    public class NotificationValidator : AbstractValidator<NotificationPersist>
    {
        public NotificationValidator()
        {
            // The user ID is required
            RuleFor(notification => notification.UserId)
                .Cascade(CascadeMode.Stop)
                .Must(RuleFluentValidation.IsObjectId)
                .WithMessage("The user ID is not in the correct format.");

            // The notification type is required and must be valid
            RuleFor(notification => notification.Type)
                .Cascade(CascadeMode.Stop)
                .IsInEnum()
                .WithMessage("The notification type must be valid. [Incoming Message: 1, AdoptionApplication: 2, Report: 3]");

            // The notification content is required
            RuleFor(notification => notification.Content)
                .Cascade(CascadeMode.Stop)
                .Length(10, 200)
                .WithMessage("The notification content must be between 10 and 200 characters.");
        }
    }
}