using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Censors;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Exceptions;
using Pawfect_Pet_Adoption_App_API.Models.AdoptionApplication;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Query;
using Pawfect_Pet_Adoption_App_API.Services.AdoptionApplicationServices;
using Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices;
using Pawfect_Pet_Adoption_App_API.Transactions;
using System.Linq;

namespace Pawfect_Pet_Adoption_App_API.Controllers
{
	[ApiController]
	[Route("api/adoption-applications")]
	public class AdoptionApplicationController : ControllerBase
	{
		private readonly IAdoptionApplicationService _adoptionApplicationService;
		private readonly ILogger<AdoptionApplicationController> _logger;
        private readonly IAuthorisationContentResolver _authorisationContentResolver;
        private readonly IBuilderFactory _builderFactory;
        private readonly ICensorFactory _censorFactory;
        private readonly IQueryFactory _queryFactory;
        private readonly AuthContextBuilder _contextBuilder;

        public AdoptionApplicationController(
			IAdoptionApplicationService adoptionApplicationService, ILogger<AdoptionApplicationController> logger,
			IAuthorisationContentResolver authorisationContentResolver,
            IBuilderFactory builderFactory,	
			ICensorFactory censorFactory,
            IQueryFactory queryFactory,
			AuthContextBuilder contextBuilder

            )
		{
			_adoptionApplicationService = adoptionApplicationService;
			_logger = logger;
            _authorisationContentResolver = authorisationContentResolver;
            _builderFactory = builderFactory;
            _censorFactory = censorFactory;
            _queryFactory = queryFactory;
            _contextBuilder = contextBuilder;
        }

		/// <summary>
		/// Query adoptionApplications.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpPost("query")]
		[Authorize]
		public async Task<IActionResult> QueryAdoptionApplications([FromBody] AdoptionApplicationLookup adoptionApplicationLookup)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			AuthContext context = _contextBuilder.OwnedFrom(adoptionApplicationLookup).AffiliatedWith(adoptionApplicationLookup).Build();
			List<String> censoredFields = await _censorFactory.Censor<AdoptionApplicationCensor>().Censor([..adoptionApplicationLookup.Fields], context);
			if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying adoption applications");

			adoptionApplicationLookup.Fields = censoredFields;
			List<Data.Entities.AdoptionApplication> datas = await adoptionApplicationLookup
																	.EnrichLookup(_queryFactory)
																	.Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
																	.CollectAsync();

			List<AdoptionApplication> models = await _builderFactory.Builder<AdoptionApplicationBuilder>()
                                                .Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
												.Build(datas, censoredFields);

			if (models == null) throw new NotFoundException("Adoption applications not found", JsonHelper.SerializeObjectFormatted(adoptionApplicationLookup), typeof(Data.Entities.AdoptionApplication));

			return Ok(models);
		}

		/// <summary>
		/// Get adoptionApplication by ID.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpGet("{id}")]
		[Authorize]
		public async Task<IActionResult> GetAdoptionApplication(String id, [FromQuery] List<String> fields)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);
		
            AdoptionApplicationLookup lookup = new AdoptionApplicationLookup();
            // Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
            lookup.Offset = 1;
            // Γενική τιμή για τη λήψη των dtos
            lookup.PageSize = 1;
            lookup.Ids = [id];

            AuthContext context = _contextBuilder.OwnedFrom(lookup).AffiliatedWith(lookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<AdoptionApplicationCensor>().Censor(BaseCensor.PrepareFieldsList([..lookup.Fields]), context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying adoption applications");

			lookup.Fields = censoredFields;
            AdoptionApplication model = (
										await _builderFactory.Builder<AdoptionApplicationBuilder>().Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
										.Build(await lookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation).CollectAsync(), censoredFields)
										)
										.FirstOrDefault();

            if (model == null) throw new NotFoundException("Adoption applications not found", JsonHelper.SerializeObjectFormatted(lookup), typeof(Data.Entities.AdoptionApplication));

            return Ok(model);
		}

		/// <summary>
		/// Persist an adoption application
		/// </summary>
		[HttpPost("persist")]
        [Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> Persist([FromBody] AdoptionApplicationPersist model, [FromQuery] List<String> fields)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			fields = BaseCensor.PrepareFieldsList(fields);

            AdoptionApplication ? adoptionApplication = await _adoptionApplicationService.Persist(model, fields);

			return Ok(adoptionApplication);
		}

		/// <summary>
		/// Delete an adoption application by ID.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 404 NotFound, 500 String
		/// </summary>
		[HttpPost("delete")]
        [Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> Delete([FromBody] String id)
		{
			// TODO: Add authorization
			if (String.IsNullOrEmpty(id) || !ModelState.IsValid) return BadRequest(ModelState);

			await _adoptionApplicationService.Delete(id);

			return Ok();
		
		}

		/// <summary>
		/// Delete multiple adoption applications by IDs.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 404 NotFound, 500 String
		/// </summary>
		[HttpPost("delete/many")]
        [Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> DeleteMany([FromBody] List<String> ids)
		{
			// TODO: Add authorization
			if (ids == null || ids.Count == 0 || !ModelState.IsValid) return BadRequest(ModelState);

			await _adoptionApplicationService.Delete(ids);

			return Ok();
		}
	}
}
