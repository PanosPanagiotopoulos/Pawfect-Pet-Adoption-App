using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;

namespace Pawfect_Pet_Adoption_App_API.Models.Authorization
{
	public class LoggedAccount
	{
		public String Token { get; set; }
		public String Phone { get; set; }
		public List<String> Roles { get; set; }
        public List<String> Permissions { get; set; }
        public Boolean IsPhoneVerified { get; set; }
		public Boolean IsEmailVerified { get; set; }
		public Boolean IsVerified { get; set; }
		public DateTime LoggedAt { get; set; }
	}
}
