using MongoDB.Driver;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;

namespace Pawfect_Pet_Adoption_App_API.Services.FilterServices
{
    public interface IFilterBuilder<TEntity, TLookup>
    {
        Task<FilterDefinition<TEntity>> Build(TLookup lookup);
    }
}
