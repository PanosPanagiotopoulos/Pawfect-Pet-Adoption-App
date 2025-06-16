using MongoDB.Bson;
using MongoDB.Driver;

namespace Main_API.Services.FilterServices
{
    public interface IFilterBuilder
    {
        Task<FilterDefinition<BsonDocument>> Build(Models.Lookups.Lookup lookup);
    }
}
