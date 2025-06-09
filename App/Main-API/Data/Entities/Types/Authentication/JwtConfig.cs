namespace Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authentication
{
	public class JwtConfig
	{
		public String Key { get; set; }
		public String Issuer { get; set; }
		public List<String> Audiences { get; set; }
	}
}
