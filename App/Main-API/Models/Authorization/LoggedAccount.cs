using Main_API.Data.Entities.EnumTypes;

namespace Main_API.Models.Authorization
{
	public class LoggedAccount
	{
        public String Email { get; set; }
        public String Phone { get; set; }
		public List<String> Roles { get; set; }
        public List<String> Permissions { get; set; }
        public Boolean IsPhoneVerified { get; set; }
		public Boolean IsEmailVerified { get; set; }
		public Boolean IsVerified { get; set; }
		public DateTime LoggedAt { get; set; }
	}
}
