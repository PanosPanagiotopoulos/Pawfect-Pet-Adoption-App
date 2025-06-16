using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Main_API.DevTools
{
    public static class MongoHelper
    {
        public static FilterDefinition<BsonDocument> ToBsonFilter<TEntity>(
            FilterDefinition<TEntity> filter
        )
        {
            BsonDocument filterBsonDocument = filter.Render(new MongoDB.Driver.RenderArgs<TEntity>
            {
                DocumentSerializer = BsonSerializer.LookupSerializer<TEntity>(),
                SerializerRegistry = BsonSerializer.SerializerRegistry
            });

            return new BsonDocumentFilterDefinition<BsonDocument>(filterBsonDocument);
        }

        public static FilterDefinition<TEntity> FromBsonFilter<TEntity>(
        FilterDefinition<BsonDocument> bsonFilter
        )
        {
            BsonDocument bsonFilterDoc = bsonFilter.Render(new MongoDB.Driver.RenderArgs<BsonDocument>
            {
                DocumentSerializer = BsonSerializer.SerializerRegistry.GetSerializer<BsonDocument>(),
                SerializerRegistry = BsonSerializer.SerializerRegistry
            });

            return new BsonDocumentFilterDefinition<TEntity>(bsonFilterDoc);
        }

    }
}
