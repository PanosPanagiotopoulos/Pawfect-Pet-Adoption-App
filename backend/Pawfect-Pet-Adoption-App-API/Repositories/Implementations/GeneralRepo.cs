using MongoDB.Bson;
using MongoDB.Driver;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;
using Pawfect_Pet_Adoption_App_API.Services;
using System.Linq.Expressions;


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

        public async Task<T> GetByIdAsync(string id)
        {
            var filter = Builders<T>.Filter.Eq("_id", new ObjectId(id));
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<bool> AddAsync(T entity)
        {
            await _collection.InsertOneAsync(entity);
            return true;
        }

        public async Task<bool> UpdateAsync(T entity)
        {
            var filter = Builders<T>.Filter.Eq("_id", entity.GetType().GetProperty("Id").GetValue(entity, null));
            var result = await _collection.ReplaceOneAsync(filter, entity);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var filter = Builders<T>.Filter.Eq("_id", new ObjectId(id));
            var result = await _collection.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }

        public async Task<bool> DeleteAsync(T entity)
        {
            var id = entity.GetType().GetProperty("Id").GetValue(entity, null).ToString();
            return await DeleteAsync(id);
        }

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await _collection.Find(predicate).AnyAsync();
        }

        public async Task<T> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _collection.Find(predicate).FirstOrDefaultAsync();
        }
    }
}