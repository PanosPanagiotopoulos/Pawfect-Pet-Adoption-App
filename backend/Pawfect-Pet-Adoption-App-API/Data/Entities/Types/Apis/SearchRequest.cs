namespace Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Apis
{
	public class SearchRequest
	{
		// Το query προς αναζήτηση στον search server
		public String Query { get; set; }
		// Το πλήθος των αποτελεσμάτων που θα επιστραφούν
		public int TopK { get; set; } = 5;
		// Η γλώσσα των αποτελεσμάτων και η γλώσσα στην οποία θα εξεταστεί το query
		public String Lang { get; set; }
	}
}
