using MongoDB.Bson;
using MongoDB.Driver;

namespace Pawfect_Notifications.Services.FilterServices
{
    public interface IFilterBuilder
    {
        Task<FilterDefinition<BsonDocument>> Build(Models.Lookups.Lookup lookup);
    }
}
