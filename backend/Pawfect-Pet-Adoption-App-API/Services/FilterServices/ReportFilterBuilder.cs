using MongoDB.Driver;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Query;

namespace Pawfect_Pet_Adoption_App_API.Services.FilterServices
{
    public class ReportFilterBuilder: IFilterBuilder<Report, ReportLookup>
    {
        private readonly IQueryFactory _queryFactory;

        public ReportFilterBuilder
        (
            IQueryFactory queryFactory
        )
        {
            this._queryFactory = queryFactory;
        }

        public async Task<FilterDefinition<Report>> Build(ReportLookup lookup)
        {
            FilterDefinitionBuilder<Report> builder = Builders<Report>.Filter;
            FilterDefinition<Report> filter = builder.Empty;

            return await lookup.EnrichLookup(_queryFactory).ApplyFilters();
        }
    }
}
