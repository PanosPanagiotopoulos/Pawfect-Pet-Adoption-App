using FluentValidation;
using Pawfect_Messenger.Data.Entities.EnumTypes;
using Pawfect_Messenger.DevTools;
namespace Pawfect_Messenger.Models.Conversation
{
    public class ConversationValidator : AbstractValidator<ConversationPersist>
    {
        public ConversationValidator()
        {
            // The ID is optional for new conversations but must be valid ObjectId if provided
            When(conversation => !String.IsNullOrEmpty(conversation.Id), () =>
            {
                RuleFor(conversation => conversation.Id)
                    .Cascade(CascadeMode.Stop)
                    .Must(RuleFluentValidation.IsObjectId)
                    .WithMessage("The conversation ID is not in the correct format.");
            });

            // The conversation type is required and must be valid enum
            RuleFor(conversation => conversation.Type)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .WithMessage("The conversation type is not valid.")
                .IsInEnum()
                .WithMessage("The conversation type is not valid.");

            // The participants list is required
            RuleFor(conversation => conversation.Participants)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .WithMessage("The participants list is required.")
                .NotEmpty()
                .WithMessage("At least one participant is required.");

            // Validate participants based on conversation type
            When(conversation => conversation.Type == ConversationType.Direct, () =>
            {
                RuleFor(conversation => conversation.Participants)
                    .Must(participants => participants != null && participants.Count == 2)
                    .WithMessage("Direct conversations must have exactly 2 participants.");
            });

            // Each participant ID must be valid ObjectId
            RuleForEach(conversation => conversation.Participants)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Participant ID cannot be empty.")
                .Must(RuleFluentValidation.IsObjectId)
                .WithMessage("The participant ID '{PropertyValue}' is not in the correct format.");

            // Ensure no duplicate participants
            RuleFor(conversation => conversation.Participants)
                .Must(participants => participants == null || participants.Count == participants.Distinct().Count())
                .WithMessage("Duplicate participants are not allowed.");

            // The creator ID is required
            RuleFor(conversation => conversation.CreatedBy)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("The creator ID is required.")
                .Must(RuleFluentValidation.IsObjectId)
                .WithMessage("The creator ID is not in the correct format.");

            // The creator must be one of the participants
            RuleFor(conversation => conversation)
                .Must(conversation => conversation.Participants != null && conversation.Participants.Contains(conversation.CreatedBy))
                .WithMessage("The creator must be one of the conversation participants.")
                .When(conversation => !String.IsNullOrEmpty(conversation.CreatedBy));
        }
    }
}
