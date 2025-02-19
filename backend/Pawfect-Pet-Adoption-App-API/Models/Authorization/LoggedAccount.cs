using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;

namespace Pawfect_Pet_Adoption_App_API.Models.Authorization
{
	public class LoggedAccount
	{
		public String Token { get; set; }
		public UserRole Role { get; set; }
		public DateTime LoggedAt { get; set; }
	}
}
