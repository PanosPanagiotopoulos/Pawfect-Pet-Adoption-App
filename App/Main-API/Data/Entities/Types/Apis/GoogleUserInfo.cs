using Newtonsoft.Json;

namespace Main_API.Data.Entities.Types.Apis
{
	// Μοντέλο JSON για τα δεδομένα χρήστη του Google
	public class GoogleUserInfo
	{
		[JsonProperty("sub")]
		public String Sub { get; set; }

		[JsonProperty("email")]
		public String Email { get; set; }

		[JsonProperty("name")]
		public String Name { get; set; }

		[JsonProperty("phone_number")]
		public String PhoneNumber { get; set; }

		[JsonProperty("address")]
		public String Address { get; set; }
	}
}
