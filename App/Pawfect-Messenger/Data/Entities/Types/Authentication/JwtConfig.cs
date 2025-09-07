namespace Pawfect_Messenger.Data.Entities.Types.Authentication
{
	public class JwtConfig
	{
		public String Key { get; set; }
		public String Issuer { get; set; }
		public List<String> Audiences { get; set; }
        public int RefreshTokenExpiration { get; set; }
    }
}
