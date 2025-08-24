using Microsoft.Extensions.Options;

using MongoDB.Bson;
using MongoDB.Driver;
using Pawfect_Notifications.Data.Entities.Types.Mongo;
using System;

namespace Pawfect_Notifications.Services.MongoServices
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

        #region Indexes
        public async Task SetupSearchIndexesAsync()
        {
            
        }
        private async Task SetupVectorSearchIndexAsync(IMongoCollection<BsonDocument> collection)
        {
            
        }

        private async Task SetupSemanticTextSearchIndexAsync(IMongoCollection<BsonDocument> collection)
        {
        }

        public async Task SetupPlainTextIndexesAsync()
        {
           
        }

        private async Task<bool> CheckRegularIndexExistsAsync(IMongoCollection<BsonDocument> collection, string indexName)
        {
            IAsyncCursor<BsonDocument> indexes = await collection.Indexes.ListAsync();
            List<BsonDocument> indexList = await indexes.ToListAsync();

            return indexList.Any(index => index["name"].AsString == indexName);
        }

        public async Task SetupSynonymsCollectionAsync()
        {
        }
        private async Task CreateSearchIndexAsync(IMongoCollection<BsonDocument> collection, BsonDocument indexDefinition)
        {
            try
            {
                BsonDocument command = new BsonDocument
                 {
                     { "createSearchIndexes", collection.CollectionNamespace.CollectionName },
                     { "indexes", new BsonArray { indexDefinition } }
                 };

                await _db.RunCommandAsync<BsonDocument>(command);

                string indexName = indexDefinition["name"].AsString;
                _logger.LogInformation($"Search index creation initiated: {indexName}");

                _logger.LogInformation($"Note: Search index '{indexName}' is being built asynchronously and may take a few minutes to become available");
            }
            catch (MongoCommandException ex)
            {
                if (ex.Code == 68) // CommandNotFound - likely not Atlas or Atlas Search not enabled
                {
                    _logger.LogWarning("Atlas Search not available. Please ensure you're using MongoDB Atlas with Search enabled.");
                    throw new InvalidOperationException("MongoDB Atlas Search is required for this functionality. Please deploy to Atlas and enable Search.", ex);
                }
                else if (ex.CodeName == "IndexAlreadyExists")
                {
                    var indexName = indexDefinition["name"].AsString;
                    _logger.LogInformation($"Search index '{indexName}' already exists");
                }
                else
                {
                    throw;
                }
            }
        }

  

        #endregion

        #region Helpers

        private IMongoCollection<T> FindCollection<T>() => _db.GetCollection<T>(this.ExtractCollectionName(typeof(T)));

        private IMongoCollection<BsonDocument> FindCollection(Type tEntity) => _db.GetCollection<BsonDocument>(this.ExtractCollectionName(tEntity));

        private string ExtractCollectionName(Type tEntity)
        {
            string typeName = tEntity.Name;
            return typeName.EndsWith("s", StringComparison.OrdinalIgnoreCase)
                ? typeName.ToLowerInvariant()
                : typeName.ToLowerInvariant() + "s";
        }


        /// <summary>
        /// Διαγράφει όλη τη βάση. ** TESTING **
        /// </summary>
        public async Task DropAll()
        {
            List<String> collectionNames = _db.ListCollectionNames().ToList();

            foreach (String collectionName in collectionNames)
            {
                IMongoCollection<BsonDocument> collection = _db.GetCollection<BsonDocument>(collectionName);

                // Drop all regular indexes
                List<BsonDocument> indexes = collection.Indexes.List().ToList();
                foreach (BsonDocument index in indexes)
                {
                    String indexName = index["name"].AsString;
                    if (indexName != "_id_")
                    {
                        collection.Indexes.DropOne(indexName);
                    }
                }

                // Delete all documents
                collection.DeleteMany(FilterDefinition<BsonDocument>.Empty);
            }

            #endregion
        }
    }
}
