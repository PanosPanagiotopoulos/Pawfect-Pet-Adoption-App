﻿using MongoDB.Bson;
using MongoDB.Driver;
using Main_API.Repositories.Interfaces;
using Main_API.Services.MongoServices;
using System.Linq.Expressions;

namespace Main_API.Repositories.Implementations
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
            List<T> entities = new List<T> { entity };
            List<String> ids = await AddManyAsync(entities, session);
            return ids.FirstOrDefault() ?? throw new InvalidOperationException("Failed to add entity to collection.");
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
            List<String> ids = await UpdateManyAsync(new List<T> { entity }, session);
            return ids.FirstOrDefault() ?? throw new InvalidOperationException("Failed to update entity.");
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

            if (result.ModifiedCount != entities.Count)
            {
                throw new InvalidOperationException($"Expected to update {entities.Count} entities, but modified {result.ModifiedCount}.");
            }

            return entities.Select(GetEntityId).ToList();
        }

        public async Task<Boolean> DeleteAsync(String id, IClientSessionHandle session = null)
        {
            List<Boolean> results = await DeleteAsync(new List<String> { id }, session);
            return results.FirstOrDefault();
        }

        public async Task<Boolean> DeleteAsync(T entity, IClientSessionHandle session = null)
        {
            String id = GetEntityId(entity);
            return await DeleteAsync(id, session);
        }

        public async Task<List<Boolean>> DeleteAsync(List<String> ids, IClientSessionHandle session = null)
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

        public async Task<List<Boolean>> DeleteAsync(List<T> entities, IClientSessionHandle session = null)
        {
            if (entities == null || !entities.Any())
            {
                throw new ArgumentNullException("No entities provided for deletion.");
            }

            List<String> ids = entities.Select(GetEntityId).ToList();
            return await DeleteAsync(ids, session);
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