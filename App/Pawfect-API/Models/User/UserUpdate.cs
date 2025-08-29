namespace Pawfect_Pet_Adoption_App_API.Models.User
{
    public class UserUpdate
    {
        public String Id { get; set; }

        public String Email { get; set; }

        public String FullName { get; set; }

        public String Phone { get; set; }

        public Location Location { get; set; }

        public String ProfilePhotoId { get; set; }
    }
}
