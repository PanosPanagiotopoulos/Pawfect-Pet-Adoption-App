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
        public AdoptionApplicationChanged AdoptionApplicationChanged { get; set; }
        public AdoptionApplicationReceived AdoptionApplicationReceived { get; set; }

    }
    public class VerificationEmailPlaceholders
    {
        public String FirstName { get; set; }
        public String VerificationToken { get; set; }
    }

    public class ResetPasswordEmailPlaceholders
    {
        public String FirstName { get; set; }
        public String ResetToken { get; set; }
    }

    public class  OtpPasswordPlaceholders
    {
        public String OtpPassword { get; set; }
    }

    public class AdoptionApplicationChanged
    {
        public String AnimalName { get; set; }
        public String ApplicationStatus { get; set; }
        public String ApplicationId { get; set; }
    }
    public class AdoptionApplicationReceived
    {
        public String AnimalName { get; set; }
        public String UserName { get; set; }
        public String ApplicationId { get; set; }
    }


}
