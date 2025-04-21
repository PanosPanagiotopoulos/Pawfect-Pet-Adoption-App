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

		public GeneralRepo(MongoDbService dbService)
		{
			this._db = dbService.db;
			this._collection = dbService.GetCollection<T>();
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

		// Bulk AddManyAsync
		public async Task<List<String>> AddManyAsync(List<T> entities)
		{
			if (entities == null || !entities.Any())
				throw new ArgumentException("No entities provided for insertion.");

			await _collection.InsertManyAsync(entities);
			return entities.Select(e => e.GetType().GetProperty("Id")?.GetValue(e)?.ToString() ?? throw new MongoException("Couldn't retrieve ID from entity")).ToList();
		}

		// Bulk UpdateManyAsync
		public async Task<List<String>> UpdateManyAsync(List<T> entities)
		{
			if (entities == null || !entities.Any())
				throw new ArgumentException("No entities provided for update.");

			List<WriteModel<T>> updates = new List<WriteModel<T>>();
			foreach (var entity in entities)
			{
				PropertyInfo idProperty = entity.GetType().GetProperty("Id");
				if (idProperty == null)
					throw new MongoException("Entity does not contain an 'Id' property");

				String idValue = idProperty.GetValue(entity)?.ToString();
				if (String.IsNullOrEmpty(idValue))
					throw new MongoException("Id value is missing or null");

				FilterDefinition<T> filter = Builders<T>.Filter.Eq("_id", new ObjectId(idValue));
				updates.Add(new ReplaceOneModel<T>(filter, entity));
			}

			BulkWriteResult<T> result = await _collection.BulkWriteAsync(updates);
			if (result.ModifiedCount != entities.Count)
				throw new MongoException("Not all entities were updated successfully.");

			return entities.Select(e => e.GetType().GetProperty("Id").GetValue(e).ToString()).ToList();
		}

		public async Task<Boolean> DeleteAsync(String id)
		{
			FilterDefinition<T> filter = Builders<T>.Filter.Eq("_id", new ObjectId(id));
			DeleteResult result = await _collection.DeleteOneAsync(filter);
			return result.DeletedCount > 0;
		}

		public async Task<Boolean> DeleteAsync(T entity)
		{
			String? id = entity.GetType().GetProperty("Id").GetValue(entity, null).ToString();
			return await DeleteAsync(id);
		}

		public async Task<Boolean> ExistsAsync(Expression<Func<T, Boolean>> predicate)
		{
			try
			{
				return await _collection.Find(predicate).AnyAsync();
			}
			catch (FormatException)
			{
				return false;
			}

		}

		public async Task<T> FindAsync(Expression<Func<T, Boolean>> predicate)
		{
			try
			{
				return await _collection.Find(predicate).FirstOrDefaultAsync();
			}
			catch (FormatException)
			{
				return null;
			}
		}

		public async Task<T> FindAsync(Expression<Func<T, Boolean>> predicate, List<String> fields)
		{
			try
			{
				ProjectionDefinition<T> projection = Builders<T>.Projection.Include(fields.First());
				foreach (String field in fields.Skip(1))
				{
					projection = projection.Include(field);
				}

				return await _collection.Find(predicate)
					.Project<T>(projection)
					.FirstOrDefaultAsync();
			}
			catch (FormatException)
			{
				return null;
			}
		}
	}
}