using MongoDB.Bson;
using MongoDB.Driver;

namespace Pawfect_Pet_Adoption_App_API.Services.FilterServices
{
    public interface IFilterBuilder
    {
        Task<FilterDefinition<BsonDocument>> Build(Models.Lookups.Lookup lookup);
    }
}
