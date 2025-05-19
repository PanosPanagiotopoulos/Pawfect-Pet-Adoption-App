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

        public GeneralRepo
		(
			MongoDbService dbService,
            IHttpContextAccessor httpContextAccessor
        )
		{
			_db = dbService.db;
			_collection = dbService.GetCollection<T>();
            _httpContextAccessor = httpContextAccessor;
        }
		// Singular AddAsync (reuses AddManyAsync)
		public async Task<String> AddAsync(T entity)
		{
			List<String> ids = await AddManyAsync(new List<T> { entity });
			return ids.FirstOrDefault() ?? throw new MongoException("Couldn't add to collection");
		}

		// Singular UpdateAsync (reuses UpdateManyAsync)
		public async Task<String> UpdateAsync(T entity)
		{
			List<String> ids = await UpdateManyAsync(new List<T> { entity });
			return ids.FirstOrDefault();
		}

        public async Task<List<String>> AddManyAsync(List<T> entities)
        {
            if (entities == null || entities.Count == 0)
                throw new ArgumentException("No entities provided for insertion.");

            IClientSessionHandle session = this.Session();
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

            return [.. entities.Select(e => e.GetType().GetProperty("Id")?.GetValue(e)?.ToString()
                ?? throw new MongoException("Couldn't retrieve ID from entity"))];
        }

        public async Task<List<String>> UpdateManyAsync(List<T> entities)
        {
            if (entities == null || entities.Count == 0)
                throw new ArgumentException("No entities provided for update.");

            List<WriteModel<T>> updates = new List<WriteModel<T>>();
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

            IClientSessionHandle session = Session();
            BulkWriteResult<T> result;
            try
            {
                result = session != null
                    ? await _collection.BulkWriteAsync(session, updates)
                    : await _collection.BulkWriteAsync(updates);
            }
            catch (MongoException ex)
            {
                throw new MongoException($"Failed to update entities: {ex.Message}", ex);
            }

            if (result.ModifiedCount != entities.Count)
                throw new MongoException("Not all entities were updated successfully.");

            return [.. entities.Select(e => e.GetType().GetProperty("Id").GetValue(e).ToString())];
        }

        public async Task<Boolean> DeleteAsync(String id)
		{
			List<Boolean> results = await this.DeleteAsync(new List<String> { id });
			return results.FirstOrDefault();
		}
		public async Task<Boolean> DeleteAsync(T entity)
		{
			String id = entity.GetType().GetProperty("Id")?.GetValue(entity)?.ToString();
			if (String.IsNullOrEmpty(id))
				throw new MongoException("Entity does not contain a valid 'Id' property");
			return await this.DeleteAsync(id);
		}

        public async Task<List<Boolean>> DeleteAsync(List<String> ids)
        {
            if (ids == null || ids.Count == 0)
                throw new ArgumentException("No IDs provided for deletion.");

            List<String> validIds = [.. ids.Where(id => ObjectId.TryParse(id, out _))];
            if (ids.Count != validIds.Count)
                throw new MongoException("Not all objectIds where valid for deletion.");

            FilterDefinition<T> filter = Builders<T>.Filter.In("_id", validIds.Select(id => new ObjectId(id)));

            // Use the session if available
            IClientSessionHandle session = this.Session();
            DeleteResult result = session != null ? await _collection.DeleteManyAsync(session, filter) : await _collection.DeleteManyAsync(filter);

            // Return a list indicating which IDs were deleted
            return [.. ids.Select(id => validIds.Contains(id) && result.DeletedCount > 0)];
        }

        public async Task<List<Boolean>> DeleteAsync(List<T> entities)
        {
            if (entities == null || entities.Count == 0)
                throw new ArgumentException("No entities provided for deletion.");

            List<String> ids = [.. entities
                .Select(e => e.GetType().GetProperty("Id")?.GetValue(e)?.ToString())
                .Where(id => !String.IsNullOrEmpty(id))];

            if (ids.Count != entities.Count)
                throw new MongoException("Not all ids where valid to delete.");

            return await DeleteAsync(ids);
        }
		public async Task<Boolean> ExistsAsync(Expression<Func<T, Boolean>> predicate) => await _collection.Find(predicate).AnyAsync();

		public async Task<T> FindAsync(Expression<Func<T, Boolean>> predicate) => await _collection.Find(predicate).FirstOrDefaultAsync();

		public async Task<T> FindAsync(Expression<Func<T, Boolean>> predicate, List<String> fields)
		{
			ProjectionDefinition<T> projection = Builders<T>.Projection.Include(fields.First());
			foreach (String field in fields.Skip(1))
				projection = projection.Include(field);

			return await _collection.Find(predicate)
				.Project<T>(projection)
				.FirstOrDefaultAsync();
		}

        private IClientSessionHandle Session() => _httpContextAccessor.HttpContext?.Items["MongoSession"] as IClientSessionHandle;
    }
}