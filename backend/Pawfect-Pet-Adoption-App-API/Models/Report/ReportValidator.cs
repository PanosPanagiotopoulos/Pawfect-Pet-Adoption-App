using FluentValidation;
using Pawfect_Pet_Adoption_App_API.DevTools;

namespace Pawfect_Pet_Adoption_App_API.Models.Report
{
    public class ReportValidator : AbstractValidator<ReportPersist>
    {
        public ReportValidator()
        {
            // Το ID του χρήστη που έκανε την αναφορά είναι απαραίτητο
            RuleFor(report => report.ReporterId)
                .Cascade(CascadeMode.Stop)
                .Must(RuleFluentValidation.IsObjectId)
                .WithMessage("Το ID του χρήστη που έκανε την αναφορά είναι λάθος.");

            // Το ID του χρήστη που έλαβε την αναφορά είναι απαραίτητο
            RuleFor(report => report.ReportedId)
                .Cascade(CascadeMode.Stop)
                .Must(RuleFluentValidation.IsObjectId)
                .WithMessage("Το ID του χρήστη που έλαβε την αναφορά είναι λάθος.");

            // Ο τύπος της αναφοράς είναι απαραίτητος
            RuleFor(report => report.Type)
                .Cascade(CascadeMode.Stop)
                .IsInEnum()
                .WithMessage("Ο τύπος της αναφοράς πρέπει να είναι έγκυρος. [ InAppropriateMessage: 1 , Other: 2 ]");

            // Η περιγραφή της αναφοράς είναι απαραίτητη
            RuleFor(report => report.Reason)
                .Cascade(CascadeMode.Stop)
                .Length(10, 200)
                .WithMessage("Η περιγραφή της αναφοράς πρέπει να είναι μεταξύ 10-200 χαρακτήρες.");

            // Ελέγξτε αν η κατάσταση επαλήθευσης δεν αποστέλλεται κατά τη δημιουργία
            RuleFor(report => report.Status)
            .Cascade(CascadeMode.Stop)
            .IsInEnum()
            .WithMessage("Ο τύπος της αναφοράς πρέπει να είναι έγκυρος. [ Pending: 1, Resolved : 2 ]");
        }
    }
}
