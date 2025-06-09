using FluentValidation;
using Pawfect_Pet_Adoption_App_API.DevTools;

namespace Pawfect_Pet_Adoption_App_API.Models.Report
{
    public class ReportValidator : AbstractValidator<ReportPersist>
    {
        public ReportValidator()
        {
            // The ID of the user who made the report is required
            RuleFor(report => report.ReporterId)
                .Cascade(CascadeMode.Stop)
                .Must(RuleFluentValidation.IsObjectId)
                .WithMessage("The ID of the user who made the report is incorrect.");

            // The ID of the user who received the report is required
            RuleFor(report => report.ReportedId)
                .Cascade(CascadeMode.Stop)
                .Must(RuleFluentValidation.IsObjectId)
                .WithMessage("The ID of the user who received the report is incorrect.");

            // The report type is required
            RuleFor(report => report.Type)
                .Cascade(CascadeMode.Stop)
                .IsInEnum()
                .WithMessage("The report type must be valid. [InAppropriateMessage: 1, Other: 2]");

            // The report description is required
            RuleFor(report => report.Reason)
                .Cascade(CascadeMode.Stop)
                .Length(10, 200)
                .WithMessage("The report description must be between 10 and 200 characters.");

            // Check that the verification status is not sent during creation
            RuleFor(report => report.Status)
                .Cascade(CascadeMode.Stop)
                .IsInEnum()
                .WithMessage("The report status must be valid. [Pending: 1, Resolved: 2]");
        }
    }
}