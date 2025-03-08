namespace Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Apis
{
	public class EmailPayload
	{
		// Τα δεδομένα Id, Email αποστέλονται ένα απο τα 2 για επιβεαβίωση του χρήστη που έκανε verify
		public String Id { get; set; }
		public String Email { get; set; }
		public String Token { get; set; }
	}
}
