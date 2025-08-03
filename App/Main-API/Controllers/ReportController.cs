using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Main_API.Builders;
using Main_API.Censors;
using Main_API.Data.Entities.Types.Authorization;
using Main_API.DevTools;
using Main_API.Exceptions;
using Main_API.Models.AdoptionApplication;
using Main_API.Models.Lookups;
using Main_API.Models.Report;
using Main_API.Query;
using Main_API.Services.ReportServices;
using Main_API.Transactions;
using System.Linq;
using System.Reflection;
using Main_API.Query.Queries;
using Pawfect_Pet_Adoption_App_API.Query;

namespace Main_API.Controllers
{
	[ApiController]
	[Route("api/reports")]
	public class ReportController : ControllerBase
	{
		private readonly IReportService _reportService;
		private readonly ILogger<ReportController> _logger;
        private readonly IQueryFactory _queryFactory;
        private readonly IBuilderFactory _builderFactory;
        private readonly ICensorFactory _censorFactory;
        private readonly AuthContextBuilder _contextBuilder;

        public ReportController
		(
			IReportService reportService, ILogger<ReportController> logger,
			IQueryFactory queryFactory, IBuilderFactory builderFactory,
            ICensorFactory censorFactory, AuthContextBuilder contextBuilder
        )
		{
			_reportService = reportService;
			_logger = logger;
            _queryFactory = queryFactory;
            _builderFactory = builderFactory;
            _censorFactory = censorFactory;
            _contextBuilder = contextBuilder;
        }

		[HttpPost("query")]
		[Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> QueryReports([FromBody] ReportLookup reportLookup)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

            AuthContext context = _contextBuilder.OwnedFrom(reportLookup).AffiliatedWith(reportLookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<ReportCensor>().Censor([.. reportLookup.Fields], context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying reports");

            reportLookup.Fields = censoredFields;

			ReportQuery q = reportLookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation);

            List<Data.Entities.Report> datas = await q.CollectAsync();

            List<Report> models = await _builderFactory.Builder<ReportBuilder>()
                                                .Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
                                                .Build(datas, [.. reportLookup.Fields]);

            if (models == null) throw new NotFoundException("Reports not found", JsonHelper.SerializeObjectFormatted(reportLookup), typeof(Data.Entities.Report));

			return Ok(new QueryResult<Report>()
			{
				Items = models,
				Count = await q.CountAsync()
			});
		}

		[HttpGet("{id}")]
		[Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> GetReport(String id, [FromQuery] List<String> fields)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

            ReportLookup lookup = new ReportLookup();

            // Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
            lookup.Offset = 1;
            // Γενική τιμή για τη λήψη των dtos
            lookup.PageSize = 1;
            lookup.Ids = [id];
            lookup.Fields = fields;

            AuthContext context = _contextBuilder.OwnedFrom(lookup).AffiliatedWith(lookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<ReportCensor>().Censor(BaseCensor.PrepareFieldsList([.. lookup.Fields]), context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying reports");

            lookup.Fields = censoredFields;
            Report model = (await _builderFactory.Builder<ReportBuilder>().Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
													.Build(await lookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation).CollectAsync(), 
													censoredFields))
													.FirstOrDefault();

            if (model == null) throw new NotFoundException("Report not found", JsonHelper.SerializeObjectFormatted(lookup), typeof(Data.Entities.Report));


            return Ok(model);
		}

		[HttpPost("persist")]
		[Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> Persist([FromBody] ReportPersist model, [FromQuery] List<String> fields)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			fields = BaseCensor.PrepareFieldsList(fields);

            Report? report = await _reportService.Persist(model, fields);

			return Ok(report);
		}

        [HttpPost("delete/{id}")]
        [Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> Delete([FromRoute] String id)
        {
            if (String.IsNullOrEmpty(id) || !ModelState.IsValid) return BadRequest(ModelState);

            await _reportService.Delete(id);

            return Ok();
        }
    }
}
