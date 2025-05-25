using MongoDB.Bson;
using MongoDB.Driver;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;
using Pawfect_Pet_Adoption_App_API.Services.MongoServices;
using System.Linq.Expressions;
using System.Reflection;

namespace Pawfect_Pet_Adoption_App_API.Repositories.Implementations
{
    public class GeneralRepo<T> : IGeneralRepo<T> where T : class
    {
        public IMongoDatabase _db { get; }
        public IMongoCollection<T> _collection { get; }
        private IHttpContextAccessor _httpContextAccessor { get; }

        public GeneralRepo(
            MongoDbService dbService,
            IHttpContextAccessor httpContextAccessor)
        {
            _db = dbService.db;
            _collection = dbService.GetCollection<T>();
            _httpContextAccessor = httpContextAccessor;
        }

        // Singular AddAsync (reuses AddManyAsync)
        public async Task<String> AddAsync(T entity, IClientSessionHandle session = null)
        {
            session ??= this.Session(); // Use provided session or get from HttpContext
            List<String> ids = await AddManyAsync(new List<T> { entity }, session);
            return ids.FirstOrDefault() ?? throw new MongoException("Couldn't add to collection");
        }

        // Multiple AddManyAsync
        public async Task<List<String>> AddManyAsync(List<T> entities, IClientSessionHandle session = null)
        {
            if (entities == null || entities.Count == 0)
                throw new ArgumentException("No entities provided for insertion.");

            session ??= this.Session(); // Use provided session or get from HttpContext
            try
            {
                if (session != null)
                    await _collection.InsertManyAsync(session, entities);
                else
                    await _collection.InsertManyAsync(entities);
            }
            catch (MongoException ex)
            {
                throw new MongoException($"Failed to insert entities: {ex.Message}", ex);
            }

            return entities.Select(e => e.GetType().GetProperty("Id")?.GetValue(e)?.ToString()
                ?? throw new MongoException("Couldn't retrieve ID from entity")).ToList();
        }

        // Singular UpdateAsync (reuses UpdateManyAsync)
        public async Task<String> UpdateAsync(T entity, IClientSessionHandle session = null)
        {
            session ??= this.Session(); // Use provided session or get from HttpContext
            List<String> ids = await UpdateManyAsync(new List<T> { entity }, session);
            return ids.FirstOrDefault();
        }

        // Multiple UpdateManyAsync
        public async Task<List<String>> UpdateManyAsync(List<T> entities, IClientSessionHandle session = null)
        {
            if (entities == null || entities.Count == 0)
                throw new ArgumentException("No entities provided for update.");

            List<WriteModel<T>> updates = new();
            foreach (T entity in entities)
            {
                PropertyInfo idProperty = entity.GetType().GetProperty("Id");
                if (idProperty == null)
                    throw new MongoException("Entity does not contain an 'Id' property");

                String idValue = idProperty.GetValue(entity)?.ToString();
                if (String.IsNullOrEmpty(idValue))
                    throw new MongoException("Id value is missing or null");

                if (!ObjectId.TryParse(idValue, out _))
                    throw new ArgumentException($"Invalid ObjectId: {idValue}");

                FilterDefinition<T> filter = Builders<T>.Filter.Eq("_id", new ObjectId(idValue));
                updates.Add(new ReplaceOneModel<T>(filter, entity));
            }

            session ??= this.Session(); // Use provided session or get from HttpContext
            BulkWriteResult<T> result;
            result = session != null
                ? await _collection.BulkWriteAsync(session, updates)
                : await _collection.BulkWriteAsync(updates);

            if (result.ModifiedCount != entities.Count)
                throw new MongoException("Not all entities were updated successfully.");

            return entities.Select(e => e.GetType().GetProperty("Id")!.GetValue(e)!.ToString()).ToList();
        }

        // Singular DeleteAsync by ID (reuses DeleteAsync for multiple IDs)
        public async Task<Boolean> DeleteAsync(String id, IClientSessionHandle session = null)
        {
            session ??= this.Session(); // Use provided session or get from HttpContext
            List<Boolean> results = await DeleteAsync(new List<String> { id }, session);
            return results.FirstOrDefault();
        }

        // Singular DeleteAsync by entity (reuses DeleteAsync by ID)
        public async Task<Boolean> DeleteAsync(T entity, IClientSessionHandle session = null)
        {
            String id = entity.GetType().GetProperty("Id")?.GetValue(entity)?.ToString();
            if (String.IsNullOrEmpty(id))
                throw new MongoException("Entity does not contain a valid 'Id' property");
            return await DeleteAsync(id, session);
        }

        // Multiple DeleteAsync by IDs
        public async Task<List<Boolean>> DeleteAsync(List<String> ids, IClientSessionHandle session = null)
        {
            if (ids == null || ids.Count == 0)
                throw new ArgumentException("No IDs provided for deletion.");

            List<String> validIds = ids.Where(id => ObjectId.TryParse(id, out _)).ToList();
            if (ids.Count != validIds.Count)
                throw new MongoException("Not all objectIds were valid for deletion.");

            FilterDefinition<T> filter = Builders<T>.Filter.In("_id", validIds.Select(id => new ObjectId(id)));

            session ??= this.Session(); // Use provided session or get from HttpContext
            DeleteResult result = session != null
                ? await _collection.DeleteManyAsync(session, filter)
                : await _collection.DeleteManyAsync(filter);

            return ids.Select(id => validIds.Contains(id) && result.DeletedCount > 0).ToList();
        }

        // Multiple DeleteAsync by entities (reuses DeleteAsync by IDs)
        public async Task<List<Boolean>> DeleteAsync(List<T> entities, IClientSessionHandle session = null)
        {
            if (entities == null || entities.Count == 0)
                throw new ArgumentException("No entities provided for deletion.");

            List<String> ids = entities
                .Select(e => e.GetType().GetProperty("Id")?.GetValue(e)?.ToString())
                .Where(id => !String.IsNullOrEmpty(id))
                .ToList();

            if (ids.Count != entities.Count)
                throw new MongoException("Not all IDs were valid to delete.");

            return await DeleteAsync(ids, session);
        }

        // ExistsAsync
        public async Task<Boolean> ExistsAsync(Expression<Func<T, Boolean>> predicate, IClientSessionHandle session = null)
        {
            session ??= this.Session(); // Use provided session or get from HttpContext
            IFindFluent<T, T> finder = session != null
                ? _collection.Find(session, predicate)
                : _collection.Find(predicate);
            return await finder.AnyAsync();
        }

        // FindAsync (without fields)
        public async Task<T> FindAsync(Expression<Func<T, Boolean>> predicate, IClientSessionHandle session = null)
        {
            session ??= this.Session(); // Use provided session or get from HttpContext
            IFindFluent<T, T> finder = session != null
                ? _collection.Find(session, predicate)
                : _collection.Find(predicate);
            return await finder.FirstOrDefaultAsync();
        }

        // FindAsync (with fields)
        public async Task<T> FindAsync(Expression<Func<T, Boolean>> predicate, List<String> fields, IClientSessionHandle session = null)
        {
            ProjectionDefinition<T> projection = Builders<T>.Projection.Include(fields.First());
            foreach (String field in fields.Skip(1))
                projection = projection.Include(field);

            session ??= this.Session(); // Use provided session or get from HttpContext
            IFindFluent<T, T> finder = session != null
                ? _collection.Find(session, predicate)
                : _collection.Find(predicate);
            return await finder.Project<T>(projection).FirstOrDefaultAsync();
        }

        // Helper method to retrieve session from HttpContext
        private IClientSessionHandle Session() => _httpContextAccessor.HttpContext?.Items["MongoSession"] as IClientSessionHandle;
    }
}