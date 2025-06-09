namespace Pawfect_Pet_Adoption_App_API.Models
{
	/// <summary>
	/// Μοντέλο για setting της Mongo βασης στο σύστημα
	/// </summary>
	public class MongoDbConfig
	{
		/// <value>
		/// Το connection String της βάσης.
		/// </value>
		public String ConnectionString { get; set; } = null!;
		/// <value>
		/// Το όνομα της βάσης
		/// </value>
		public String DatabaseName { get; set; } = null!;
	}

}
