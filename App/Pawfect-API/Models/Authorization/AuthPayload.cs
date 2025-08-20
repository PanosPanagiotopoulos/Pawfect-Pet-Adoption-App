using Pawfect_API.Data.Entities.EnumTypes;

namespace Pawfect_API.Models.Authorization
{
	public class AuthPayload
	{
		// Email and Auth Related
		public String Id { get; set; }
		public String Email { get; set; }
		public String Token { get; set; }

		// OTP Related
		public int? Otp { get; set; }
		public String Phone { get; set; }

		// Login & Reset Password Related
		public String? Password { get; set; }
		public String? ProviderAccessCode { get; set; }
		public AuthProvider LoginProvider { get; set; }
	}
}
