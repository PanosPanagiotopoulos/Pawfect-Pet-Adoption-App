using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Censors;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorization;
using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Exceptions;
using Pawfect_Pet_Adoption_App_API.Models.AdoptionApplication;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Shelter;
using Pawfect_Pet_Adoption_App_API.Query;
using Pawfect_Pet_Adoption_App_API.Services.ShelterServices;
using Pawfect_Pet_Adoption_App_API.Transactions;
using System.Linq;
using System.Reflection;

namespace Pawfect_Pet_Adoption_App_API.Controllers
{
	[ApiController]
	[Route("api/shelters")]
	public class ShelterController : ControllerBase
	{
		private readonly IShelterService _shelterService;
		private readonly ILogger<ShelterController> _logger;
        private readonly IQueryFactory _queryFactory;
        private readonly IBuilderFactory _builderFactory;
        private readonly ICensorFactory _censorFactory;
        private readonly AuthContextBuilder _contextBuilder;

        public ShelterController
			(
				IShelterService shelterService, ILogger<ShelterController> logger,
				IQueryFactory queryFactory, IBuilderFactory builderFactory,
                ICensorFactory censorFactory, AuthContextBuilder contextBuilder
            )
		{
			_shelterService = shelterService;
			_logger = logger;
            _queryFactory = queryFactory;
            _builderFactory = builderFactory;
            _censorFactory = censorFactory;
            _contextBuilder = contextBuilder;
        }

		/// <summary>
		/// Query ζώων.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpPost("query")]
		[Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> QueryShelters([FromBody] ShelterLookup shelterLookup)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

            AuthContext context = _contextBuilder.OwnedFrom(shelterLookup).AffiliatedWith(shelterLookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<ShelterCensor>().Censor([.. shelterLookup.Fields], context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying shelters");

            shelterLookup.Fields = censoredFields;
            List<Data.Entities.Shelter> datas = await shelterLookup
                                                    .EnrichLookup(_queryFactory)
                                                    .Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
                                                    .CollectAsync();

            List<Shelter> models = await _builderFactory.Builder<ShelterBuilder>()
                                                .Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
                                                .Build(datas, [.. shelterLookup.Fields]);

            if (models == null) throw new NotFoundException("Shelters not found", JsonHelper.SerializeObjectFormatted(shelterLookup), typeof(Data.Entities.Shelter));

            return Ok(models);
		}

		/// <summary>
		/// Λήψη ζώου με βάση το ID.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpGet("{id}")]
		[Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> GetShelter(String id, [FromQuery] List<String> fields)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

            ShelterLookup lookup = new ShelterLookup();

            // Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
            lookup.Offset = 1;
            // Γενική τιμή για τη λήψη των dtos
            lookup.PageSize = 1;
            lookup.Ids = [id];
            lookup.Fields = fields;

            AuthContext context = _contextBuilder.OwnedFrom(lookup).AffiliatedWith(lookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<ShelterCensor>().Censor(BaseCensor.PrepareFieldsList([.. lookup.Fields]), context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying shelters");

            lookup.Fields = censoredFields;
            Shelter model = (await _builderFactory.Builder<ShelterBuilder>().Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
									.Build(await lookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation).CollectAsync(), censoredFields))
									.FirstOrDefault();

            if (model == null) throw new NotFoundException("Shelter not found", JsonHelper.SerializeObjectFormatted(lookup), typeof(Data.Entities.Shelter));


            return Ok(model);
		}

		/// <summary>
		/// Persist an shelter.
		/// </summary>
		[HttpPost("persist")]
		[Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> Persist([FromBody] ShelterPersist model, [FromQuery] List<String> fields)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			fields = BaseCensor.PrepareFieldsList(fields);

            Shelter shelter = await _shelterService.Persist(model, fields);

			return Ok(shelter);
			
		}

		/// <summary>
		/// Delete a shelter by ID.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 404 NotFound, 500 String
		/// </summary>
		[HttpPost("delete")]
		[Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> Delete([FromBody] String id)
		{
			if (String.IsNullOrEmpty(id) || !ModelState.IsValid) return BadRequest(ModelState);

			await _shelterService.Delete(id);

			return Ok();
		}

		/// <summary>
		/// Delete multiple shelters by IDs.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 404 NotFound, 500 String
		/// </summary>
		[HttpPost("delete/many")]
		[Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> DeleteMany([FromBody] List<String> ids)
		{
			if (ids == null || ids.Count == 0 || !ModelState.IsValid) return BadRequest(ModelState);

			await _shelterService.Delete(ids);

			return Ok();
		}
	}
}
