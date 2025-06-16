using Microsoft.Extensions.Options;

using MongoDB.Bson;
using MongoDB.Driver;
using Main_API.Models;

namespace Main_API.Services.MongoServices
{
	/// <summary>
	///   Εισαγωγή της βάσης στο πρόγραμμα
	///   Γενική μέθοδος για χρήση οποιουδήποτε collection
	/// </summary>
	public class MongoDbService
	{
        private readonly IHttpContextAccessor _httpContextAccessor;
        public IMongoDatabase _db { get; }

		public MongoDbService
		(
			IOptions<MongoDbConfig> settings, 
			IMongoClient client,
            IHttpContextAccessor httpContextAccessor 
		)
		{
			_db = client.GetDatabase(settings.Value.DatabaseName);
            _httpContextAccessor = httpContextAccessor;
        }
        public IMongoCollection<T> GetCollection<T>()
		{
            IMongoCollection<T> collection = this.FindCollection<T>();

            if (collection == null) throw new InvalidOperationException("No collection found");

            return new SessionScopedMongoCollection<T>(collection, _httpContextAccessor);
        }

        public IMongoCollection<BsonDocument> GetCollection(Type tEntity)
        {
            IMongoCollection<BsonDocument> collection = this.FindCollection(tEntity);

            if (collection == null) throw new InvalidOperationException("No collection found");

            return new SessionScopedMongoCollection<BsonDocument>(collection, _httpContextAccessor);
        }

        private IMongoCollection<T> FindCollection<T>() => _db.GetCollection<T>(this.ExtractCollectionName(typeof(T)));

        private IMongoCollection<BsonDocument> FindCollection(Type tEntity) => _db.GetCollection<BsonDocument>(this.ExtractCollectionName(tEntity));

        private String ExtractCollectionName(Type tEntity)
        {
            String typeName = tEntity.Name;
            return typeName.EndsWith("s", StringComparison.OrdinalIgnoreCase)
                ? typeName.ToLowerInvariant()
                : typeName.ToLowerInvariant() + "s";
        }

        /// <summary>
        /// Διαγράφει όλη τη βάση. ** TESTING **
        /// </summary>
        public void DropAllCollections()
		{
			// Get the list of collections in the database
			List<String> excludedInDrop = ["users", "shelters", "files"];
			//List<String> collectionNames = db.ListCollectionNames().ToList().Where(cName => !excludedInDrop.Contains(cName)).ToList();
            List<String> collectionNames = _db.ListCollectionNames().ToList();

            foreach (String collectionName in collectionNames)
			{
				IMongoCollection<BsonDocument> collection = _db.GetCollection<BsonDocument>(collectionName);
				collection.DeleteMany(FilterDefinition<BsonDocument>.Empty);
			}
		}
	}

}
