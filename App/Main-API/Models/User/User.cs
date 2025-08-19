using Main_API.Data.Entities.EnumTypes;

namespace Main_API.Models.User
{
	public class User
	{
		public String? Id { get; set; }
		public String Email { get; set; }
		public String FullName { get; set; }
		public List<UserRole> Roles { get; set; }
		public String Phone { get; set; }
		public Location? Location { get; set; }
		public Shelter.Shelter? Shelter { get; set; }
		public File.File ProfilePhoto { get; set; }
        public AuthProvider AuthProvider { get; set; }
        public String AuthProviderId { get; set; }
        public Boolean? IsVerified { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
        public List<AdoptionApplication.AdoptionApplication> RequestedAdoptionApplications { get; set; }
    }
}
