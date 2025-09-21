namespace Pawfect_API.Models.User
{
    public class UserUpdate
    {
        public String Id { get; set; }

        public String Email { get; set; }

        public String FullName { get; set; }

        public String Phone { get; set; }

        public Location Location { get; set; }

        public String ProfilePhotoId { get; set; }

        public Boolean? IsVerified { get; set; }
        public Boolean? HasEmailVerified { get; set; }
        public Boolean? HasPhoneVerified { get; set; }
    }
}
