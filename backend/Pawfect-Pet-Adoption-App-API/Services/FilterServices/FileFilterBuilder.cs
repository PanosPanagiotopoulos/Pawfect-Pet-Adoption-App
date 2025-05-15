using MongoDB.Driver;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Query;

namespace Pawfect_Pet_Adoption_App_API.Services.FilterServices
{
    public class FileFilterBuilder: IFilterBuilder<Data.Entities.File, FileLookup>
    {
        private readonly IQueryFactory _queryFactory;

        public FileFilterBuilder
        (
            IQueryFactory queryFactory
        )
        {
            this._queryFactory = queryFactory;
        }

        public async Task<FilterDefinition<Data.Entities.File>> Build(FileLookup lookup)
        {
            FilterDefinitionBuilder< Data.Entities.File> builder = Builders< Data.Entities.File>.Filter;
            FilterDefinition<Data.Entities.File> filter = builder.Empty;

            return await lookup.EnrichLookup(_queryFactory).ApplyFilters();
        }
    }
}
