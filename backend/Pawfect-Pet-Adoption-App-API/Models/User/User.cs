using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;

namespace Pawfect_Pet_Adoption_App_API.Models.User
{
	public class User
	{
		public String? Id { get; set; }
		public String Email { get; set; }
		public String FullName { get; set; }
		public UserRole? Role { get; set; }
		public String Phone { get; set; }
		public Location? Location { get; set; }
		public Shelter.Shelter? Shelter { get; set; }
		public AuthProvider? AuthProvider { get; set; }
		public String AuthProviderId { get; set; }
		public File.File ProfilePhoto { get; set; }
		public Boolean? IsVerified { get; set; }
		public Boolean? HasPhoneVerified { get; set; }
		public Boolean? HasEmailVerified { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}
}
