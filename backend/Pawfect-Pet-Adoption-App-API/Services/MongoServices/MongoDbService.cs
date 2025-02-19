using Microsoft.Extensions.Options;

using MongoDB.Bson;
using MongoDB.Driver;

using Pawfect_Pet_Adoption_App_API.Models;

namespace Pawfect_Pet_Adoption_App_API.Services.MongoServices
{
	/// <summary>
	///   Εισαγωγή της βάσης στο πρόγραμμα
	///   Γενική μέθοδος για χρήση οποιουδήποτε collection
	/// </summary>
	public class MongoDbService
	{
		public IMongoDatabase db { get; }

		public MongoDbService(IOptions<MongoDbConfig> settings, IMongoClient client)
		{
			db = client.GetDatabase(settings.Value.DatabaseName);
		}

		public IMongoCollection<T> GetCollection<T>()
		{
			// Τροποποιήστε το όνομα του μοντέλου συλλογής T σε πεζά γράμματα και πληθυντικό για ασφάλεια της αντιστοίχισης ονομάτων
			return db.GetCollection<T>(!typeof(T).Name.EndsWith("s") ? typeof(T).Name.ToLower() + "s" : typeof(T).Name.ToLower());
		}

		/// <summary>
		/// Διαγράφει όλη τη βάση. Χρήση για testing
		/// </summary>
		public void DropAllCollections()
		{
			// Get the list of collections in the database
			List<String> collectionNames = db.ListCollectionNames().ToList();

			foreach (String collectionName in collectionNames)
			{
				IMongoCollection<BsonDocument> collection = db.GetCollection<BsonDocument>(collectionName);
				collection.DeleteMany(FilterDefinition<BsonDocument>.Empty);
			}
		}
	}

}
