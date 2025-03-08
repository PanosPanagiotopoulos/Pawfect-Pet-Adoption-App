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
		public async Task<String> AddAsync(T entity)
		{
			await _collection.InsertOneAsync(entity);
			String? id = entity.GetType().GetProperty("Id").GetValue(entity).ToString();
			return String.IsNullOrEmpty(id) ? throw new MongoException("Couldn't add to collection") : id;
		}

		public async Task<String> UpdateAsync(T entity)
		{
			PropertyInfo? idProperty = entity.GetType().GetProperty("Id");
			if (idProperty == null)
			{
				throw new MongoException("Entity does not contain an 'Id' property");
			}

			String? idValue = idProperty.GetValue(entity, null)?.ToString();
			if (String.IsNullOrEmpty(idValue))
			{
				throw new MongoException("Id value is missing or null");
			}

			FilterDefinition<T> filter = Builders<T>.Filter.Eq("_id", new ObjectId(idValue));
			ReplaceOneResult result = await _collection.ReplaceOneAsync(filter, entity);

			return idValue;
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
	}
}