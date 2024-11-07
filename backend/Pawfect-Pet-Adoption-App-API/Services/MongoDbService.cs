namespace Pawfect_Pet_Adoption_App_API.Services
{
    using Microsoft.Extensions.Options;
    using MongoDB.Driver;
    using Pawfect_Pet_Adoption_App_API.Database;

    /// <summary>
    ///   Εισαγωγή της βάσης στο πρόγραμμα
    ///   Γενική μέθοδος για χρήση οποιουδήποτε collection
    /// </summary>
    public class MongoDbService
    {
        public IMongoDatabase db { get; }

        public MongoDbService(IOptions<MongoDbSettings> settings, IMongoClient client)
        {
            db = client.GetDatabase(settings.Value.DatabaseName);
        }

        public IMongoCollection<T> GetCollection<T>()
        {
            // Τροποποιήστε το όνομα του μοντέλου συλλογής T σε πεζά γράμματα και πληθυντικό για ασφάλεια της αντιστοίχισης ονομάτων
            return db.GetCollection<T>((!typeof(T).Name.EndsWith("s")) ? typeof(T).Name.ToLower() + "s" : typeof(T).Name.ToLower());
        }

        /// <summary>
        /// Διαγράφει όλη τη βάση. Χρήση για testing
        /// </summary>
        public void DropAllCollections()
        {
            // Get the list of collections in the database
            var collectionNames = this.db.ListCollectionNames().ToList();

            // Drop each collection
            foreach (var collectionName in collectionNames)
            {
                this.db.DropCollection(collectionName);
            }
        }
    }

}
