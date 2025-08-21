using System.Collections.Generic;

namespace Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Apis
{
    public class NotificationApiConfig
    {
        public String SharedSecret { get; set; }
        public String FromServiceName { get; set; }
        public String NotificationEventUrl { get; set; }

        public VerificationEmailPlaceholders VerificationEmailPlaceholders { get; set; }
        public ResetPasswordEmailPlaceholders ResetPasswordEmailPlaceholders { get; set; }
        public OtpPasswordPlaceholders OtpPasswordPlaceholders { get; set; }
        public AdoptionApplicationChangedUserPlaceholders AdoptionApplicationChangedUserPlaceholders { get; set; }
        public AdoptionApplicationChangedShelterPlaceholders AdoptionApplicationChangedShelterPlaceholders { get; set; }
        public AdoptionApplicationReceivedPlaceholders AdoptionApplicationReceivedPlaceholders { get; set; }

    }
    public class VerificationEmailPlaceholders
    {
        public Guid TemplateId { get; set; }
        public String FirstName { get; set; }
        public String VerificationToken { get; set; }
    }

    public class ResetPasswordEmailPlaceholders
    {
        public Guid TemplateId { get; set; }
        public String FirstName { get; set; }
        public String ResetToken { get; set; }
    }

    public class  OtpPasswordPlaceholders
    {
        public Guid TemplateId { get; set; }
        public String OtpPassword { get; set; }
    }

    public class AdoptionApplicationChangedUserPlaceholders
    {
        public Guid TemplateId { get; set; }
        public String AnimalName { get; set; }
        public String ApplicationStatus { get; set; }
        public String ApplicationId { get; set; }
        public String UserFirstName { get; set; }
        public String ShelterName { get; set; }
    }

    public class AdoptionApplicationChangedShelterPlaceholders
    {
        public Guid TemplateId { get; set; }
        public String AnimalName { get; set; }
        public String UserFullName { get; set; }
        public String ApplicationId { get; set; }
    }
    public class AdoptionApplicationReceivedPlaceholders
    {
        public Guid TemplateId { get; set; }
        public String AnimalName { get; set; }
        public String UserName { get; set; }
        public String ApplicationId { get; set; }
    }


}
