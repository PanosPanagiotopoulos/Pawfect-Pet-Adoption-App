using Microsoft.Extensions.Options;

using MongoDB.Bson;
using MongoDB.Driver;
using Pawfect_Messenger.Data.Entities.Types.Mongo;

namespace Pawfect_Messenger.Services.MongoServices
{
    /// <summary>
    ///   Εισαγωγή της βάσης στο πρόγραμμα
    ///   Γενική μέθοδος για χρήση οποιουδήποτε collection
    /// </summary>
    public class MongoDbService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<MongoDbService> _logger;
        private readonly MongoDbConfig _config;

        public IMongoDatabase _db { get; }

        public MongoDbService
        (
            IOptions<MongoDbConfig> settings,
            IMongoClient client,
            IHttpContextAccessor httpContextAccessor,
            ILogger<MongoDbService> logger,
            IOptions<MongoDbConfig> options
        )
        {
            _db = client.GetDatabase(settings.Value.DatabaseName);
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _config = options.Value;
        }
        public IMongoCollection<T> GetCollection<T>()
        {
            IMongoCollection<T> collection = FindCollection<T>();

            if (collection == null) throw new InvalidOperationException("No collection found");

            return new SessionScopedMongoCollection<T>(collection, _httpContextAccessor);
        }

        public IMongoCollection<BsonDocument> GetCollection(Type tEntity)
        {
            IMongoCollection<BsonDocument> collection = FindCollection(tEntity);

            if (collection == null) throw new InvalidOperationException("No collection found");

            return new SessionScopedMongoCollection<BsonDocument>(collection, _httpContextAccessor);
        }

        #region Helpers

        private IMongoCollection<T> FindCollection<T>() => _db.GetCollection<T>(ExtractCollectionName(typeof(T)));

        private IMongoCollection<BsonDocument> FindCollection(Type tEntity) => _db.GetCollection<BsonDocument>(ExtractCollectionName(tEntity));

        private String ExtractCollectionName(Type tEntity)
        {
            String typeName = tEntity.Name;
            return typeName.EndsWith("s", StringComparison.OrdinalIgnoreCase)
                ? typeName.ToLowerInvariant()
                : typeName.ToLowerInvariant() + "s";
        }

        #endregion
    }
}
