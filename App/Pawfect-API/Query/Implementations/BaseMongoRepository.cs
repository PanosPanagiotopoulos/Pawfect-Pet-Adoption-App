using MongoDB.Bson;
using MongoDB.Driver;
using Pawfect_API.Repositories.Interfaces;
using Pawfect_API.Services.MongoServices;
using System.Linq.Expressions;

namespace Pawfect_API.Repositories.Implementations
{
    public class BaseMongoRepository<T> : IMongoRepository<T> where T : class
    {
        public IMongoDatabase _db { get; }
        public IMongoCollection<T> _collection { get; }

        public BaseMongoRepository(MongoDbService dbService)
        {
            _db = dbService._db;
            _collection = dbService.GetCollection<T>();
        }

        public async Task<String> AddAsync(T entity, IClientSessionHandle session = null)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            try
            {
                if (session != null)
                {
                    await _collection.InsertOneAsync(session, entity, cancellationToken: default);
                }
                else
                {
                    await _collection.InsertOneAsync(entity, cancellationToken: default);
                }
            }
            catch (MongoException ex)
            {
                throw new InvalidOperationException($"Failed to insert entity: {ex.Message}", ex);
            }

            return GetEntityId(entity);
        }

        public async Task<List<String>> AddManyAsync(List<T> entities, IClientSessionHandle session = null)
        {
            if (entities == null || !entities.Any())
            {
                throw new ArgumentNullException("No entities provided for insertion.");
            }

            try
            {
                if (session != null)
                {
                    await _collection.InsertManyAsync(session, entities, cancellationToken: default);
                }
                else
                {
                    await _collection.InsertManyAsync(entities, cancellationToken: default);
                }
            }
            catch (MongoException ex)
            {
                throw new InvalidOperationException($"Failed to insert entities: {ex.Message}", ex);
            }

            return entities.Select(GetEntityId).ToList();
        }

        public async Task<String> UpdateAsync(T entity, IClientSessionHandle session = null)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            String id = GetEntityId(entity);

            if (!ObjectId.TryParse(id, out ObjectId objectId))
                throw new ArgumentException($"Invalid ObjectId format: {id}");

            FilterDefinition<T> filter = Builders<T>.Filter.Eq("_id", objectId);

            ReplaceOneResult result;
            ReplaceOptions options = new ReplaceOptions
            {
                IsUpsert = false
            };
            try
            {
                if (session != null)
                {
                    result = await _collection.ReplaceOneAsync(session, filter, entity, options: options, cancellationToken: default);
                }
                else
                {
                    result = await _collection.ReplaceOneAsync(filter, entity, options: options, cancellationToken: default);
                }
            }
            catch (MongoException ex)
            {
                throw new InvalidOperationException($"Failed to update entity: {ex.Message}", ex);
            }

            if (!result.IsAcknowledged)
                throw new InvalidOperationException("Failed to update entity.");

            return id;
        }

        public async Task<List<String>> UpdateManyAsync(List<T> entities, IClientSessionHandle session = null)
        {
            if (entities == null || !entities.Any())
            {
                throw new ArgumentNullException("No entities provided for update.");
            }

            List<WriteModel<T>> updates = new List<WriteModel<T>>();
            foreach (T entity in entities)
            {
                String id = GetEntityId(entity);
                FilterDefinition<T> filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
                updates.Add(new ReplaceOneModel<T>(filter, entity));
            }

            BulkWriteResult<T> result = session != null
                ? await _collection.BulkWriteAsync(session, updates, cancellationToken: default)
                : await _collection.BulkWriteAsync(updates, cancellationToken: default);

           
            return entities.Select(GetEntityId).ToList();
        }

        public async Task<Boolean> DeleteAsync(String id, IClientSessionHandle session = null)
        {
            if (String.IsNullOrEmpty(id) || !ObjectId.TryParse(id, out ObjectId objectId))
                throw new InvalidOperationException("Invalid ObjectId provided for deletion.");

            FilterDefinition<T> filter = Builders<T>.Filter.Eq("_id", objectId);

            DeleteResult result;
            try
            {
                if (session != null)
                {
                    result = await _collection.DeleteOneAsync(session, filter, cancellationToken: default);
                }
                else
                {
                    result = await _collection.DeleteOneAsync(filter, cancellationToken: default);
                }
            }
            catch (MongoException ex)
            {
                throw new InvalidOperationException($"Failed to delete entity: {ex.Message}", ex);
            }

            return result.DeletedCount > 0;
        }

        public async Task<Boolean> DeleteAsync(T entity, IClientSessionHandle session = null)
        {
            String id = GetEntityId(entity);
            return await this.DeleteAsync(id, session);
        }

        public async Task<List<Boolean>> DeleteManyAsync(List<String> ids, IClientSessionHandle session = null)
        {
            if (ids == null || !ids.Any())
            {
                throw new ArgumentNullException("No IDs provided for deletion.");
            }

            List<String> validIds = ids.Where(id => ObjectId.TryParse(id, out _)).ToList();
            if (validIds.Count != ids.Count)
            {
                throw new InvalidOperationException("One or more invalid ObjectIds provided for deletion.");
            }

            FilterDefinition<T> filter = Builders<T>.Filter.In("_id", validIds.Select(ObjectId.Parse));

            DeleteResult result = session != null
                ? await _collection.DeleteManyAsync(session, filter, cancellationToken: default)
                : await _collection.DeleteManyAsync(filter, cancellationToken: default);

            return ids.Select(id => result.DeletedCount > 0).ToList();
        }

        public async Task<List<Boolean>> DeleteManyAsync(List<T> entities, IClientSessionHandle session = null)
        {
            if (entities == null || !entities.Any())
            {
                throw new ArgumentNullException("No entities provided for deletion.");
            }

            List<String> ids = entities.Select(GetEntityId).ToList();
            return await DeleteManyAsync(ids, session);
        }

        public async Task<Boolean> ExistsAsync(Expression<Func<T, Boolean>> predicate, IClientSessionHandle session = null)
        {
            IFindFluent<T, T> finder = session != null
                ? _collection.Find(session, predicate)
                : _collection.Find(predicate);
            return await finder.AnyAsync();
        }

        public async Task<T> FindAsync(Expression<Func<T, Boolean>> predicate, IClientSessionHandle session = null)
        {
            IFindFluent<T, T> finder = session != null
                ? _collection.Find(session, predicate)
                : _collection.Find(predicate);
            return await finder.FirstOrDefaultAsync();
        }

        public async Task<T> FindAsync(Expression<Func<T, Boolean>> predicate, List<String> fields, IClientSessionHandle session = null)
        {
            ProjectionDefinition<T> projection = Builders<T>.Projection.Include(fields.First());
            foreach (String field in fields.Skip(1))
            {
                projection = projection.Include(field);
            }

            IFindFluent<T, T> finder = session != null
                ? _collection.Find(session, predicate)
                : _collection.Find(predicate);
            return await finder.Project<T>(projection).FirstOrDefaultAsync();
        }

        public async Task<List<T>> FindManyAsync(Expression<Func<T, Boolean>> predicate, IClientSessionHandle session = null)
        {
            IFindFluent<T, T> finder = session != null
                ? _collection.Find(session, predicate)
                : _collection.Find(predicate);

            return await finder.ToListAsync();
        }

        /// <summary>
        /// Returns all documents that match the predicate, projecting only the specified fields.
        /// </summary>
        public async Task<List<T>> FindManyAsync(Expression<Func<T, Boolean>> predicate, List<String> fields, IClientSessionHandle session = null)
        {
            if (fields == null || fields.Count == 0)
                throw new ArgumentException("Projection fields list cannot be null or empty.", nameof(fields));

            // Build projection
            ProjectionDefinition<T> projection = Builders<T>.Projection.Include(fields[0]);
            foreach (String field in fields.Skip(1))
                projection = projection.Include(field);

            IFindFluent<T, T> finder = session != null
                ? _collection.Find(session, predicate)
                : _collection.Find(predicate);

            return await finder.Project<T>(projection).ToListAsync();
        }

        private static String GetEntityId(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            String id = entity.GetType().GetProperty("Id")?.GetValue(entity)?.ToString();
            if (String.IsNullOrEmpty(id) || !ObjectId.TryParse(id, out _))
            {
                throw new InvalidOperationException("Entity does not contain a valid 'Id' property.");
            }

            return id;
        }
    }
}