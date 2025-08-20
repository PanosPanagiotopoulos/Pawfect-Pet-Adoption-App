using FluentValidation;
using Pawfect_Notifications.DevTools;

namespace Pawfect_Notifications.Models.Notification
{
    public class NotificationValidator : AbstractValidator<NotificationEvent>
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
                .WithMessage("The notification type must be valid.");

            // The notification content Mappings
            RuleFor(notification => notification.TitleMappings)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .WithMessage("The notification title mappings must not be missing");

            RuleFor(notification => notification.ContentMappings)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .WithMessage("The notification content mappings must not be missing");

            RuleFor(notification => notification.TeplateId)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .WithMessage("The notification template id must not be missing");
        }
    }
}