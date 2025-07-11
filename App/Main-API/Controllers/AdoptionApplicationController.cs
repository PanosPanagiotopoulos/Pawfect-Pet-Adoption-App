using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Main_API.Builders;
using Main_API.Censors;
using Main_API.Data.Entities.Types.Authorization;
using Main_API.DevTools;
using Main_API.Exceptions;
using Main_API.Models.AdoptionApplication;
using Main_API.Models.Lookups;
using Main_API.Query;
using Main_API.Services.AdoptionApplicationServices;
using Main_API.Services.AuthenticationServices;
using Main_API.Transactions;
using System.Linq;
using System.Security.Claims;
using Main_API.Services.Convention;
using Main_API.Query.Queries;
using Pawfect_Pet_Adoption_App_API.Query;

namespace Main_API.Controllers
{
	[ApiController]
	[Route("api/adoption-applications")]
	public class AdoptionApplicationController : ControllerBase
	{
		private readonly IAdoptionApplicationService _adoptionApplicationService;
		private readonly ILogger<AdoptionApplicationController> _logger;
        private readonly IAuthorizationContentResolver _authorizationContentResolver;
        private readonly IBuilderFactory _builderFactory;
        private readonly ICensorFactory _censorFactory;
        private readonly IQueryFactory _queryFactory;
        private readonly AuthContextBuilder _contextBuilder;
        private readonly ClaimsExtractor _claimsExtractor;
        private readonly IConventionService _conventionService;

        public AdoptionApplicationController(
			IAdoptionApplicationService adoptionApplicationService, 
            ILogger<AdoptionApplicationController> logger,
			IAuthorizationContentResolver authorizationContentResolver,
            IBuilderFactory builderFactory,	
			ICensorFactory censorFactory,
            IQueryFactory queryFactory,
			AuthContextBuilder contextBuilder,
			ClaimsExtractor claimsExtractor,
			IConventionService conventionService

            )
		{
			_adoptionApplicationService = adoptionApplicationService;
			_logger = logger;
            _authorizationContentResolver = authorizationContentResolver;
            _builderFactory = builderFactory;
            _censorFactory = censorFactory;
            _queryFactory = queryFactory;
            _contextBuilder = contextBuilder;
            _claimsExtractor = claimsExtractor;
            _conventionService = conventionService;
        }

        /// <summary>
        /// Query adoptionApplications.
        /// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
        /// </summary>
        [HttpPost("query")]
        [Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> QueryAdoptionApplications([FromBody] AdoptionApplicationLookup adoptionApplicationLookup)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            AuthContext context = _contextBuilder.OwnedFrom(adoptionApplicationLookup).AffiliatedWith(adoptionApplicationLookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<AdoptionApplicationCensor>().Censor([.. adoptionApplicationLookup.Fields], context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying adoption applications");

            adoptionApplicationLookup.Fields = censoredFields;

            AdoptionApplicationQuery q = adoptionApplicationLookup
                .EnrichLookup(_queryFactory)
                .Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation);

            List<Data.Entities.AdoptionApplication> datas = await q.CollectAsync();

            List<AdoptionApplication> models = await _builderFactory.Builder<AdoptionApplicationBuilder>()
                .Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
                .Build(datas, censoredFields);

            if (models == null)
                throw new NotFoundException("Adoption applications not found", JsonHelper.SerializeObjectFormatted(adoptionApplicationLookup), typeof(Data.Entities.AdoptionApplication));

            return Ok(new QueryResult<AdoptionApplication>
            {
                Items = models,
                Count = await q.CountAsync()
            });
        }

        [HttpPost("query/mine/requested")]
        [Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> QueryAdoptionApplicationsMineRequested([FromBody] AdoptionApplicationLookup adoptionApplicationLookup)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            ClaimsPrincipal currentUser = _authorizationContentResolver.CurrentPrincipal();
            String userId = _claimsExtractor.CurrentUserId(currentUser);
            if (!_conventionService.IsValidId(userId)) throw new UnAuthenticatedException("User is not authenticated.");

            AuthContext context = _contextBuilder.OwnedFrom(adoptionApplicationLookup, userId).AffiliatedWith(adoptionApplicationLookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<AdoptionApplicationCensor>().Censor([.. adoptionApplicationLookup.Fields], context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying adoption applications");

			if (adoptionApplicationLookup.UserIds == null)
				adoptionApplicationLookup.UserIds = new List<String>();

			adoptionApplicationLookup.UserIds.Add(userId);
            adoptionApplicationLookup.Fields = censoredFields;

            AdoptionApplicationQuery q = adoptionApplicationLookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation);


            List<Data.Entities.AdoptionApplication> datas = await q.CollectAsync();

            List<AdoptionApplication> models = await _builderFactory.Builder<AdoptionApplicationBuilder>()
                                                .Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
                                                .Build(datas, censoredFields);

            if (models == null) throw new NotFoundException("Adoption applications not found", JsonHelper.SerializeObjectFormatted(adoptionApplicationLookup), typeof(Data.Entities.AdoptionApplication));

            return Ok(new QueryResult<AdoptionApplication>()
            {
                Items = models,
                Count = await q.CountAsync()
            });
        }

        [HttpPost("query/mine/received")]
        [Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> QueryAdoptionApplicationsMineReceived([FromBody] AdoptionApplicationLookup adoptionApplicationLookup)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            String shelterId = await _authorizationContentResolver.CurrentPrincipalShelter();
            if (!_conventionService.IsValidId(shelterId)) return null;

            AuthContext context = _contextBuilder.OwnedFrom(adoptionApplicationLookup).AffiliatedWith(adoptionApplicationLookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<AdoptionApplicationCensor>().Censor([.. adoptionApplicationLookup.Fields], context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying adoption applications");

            if (adoptionApplicationLookup.ShelterIds == null)
                adoptionApplicationLookup.ShelterIds = new List<String>();

            adoptionApplicationLookup.ShelterIds.Add(shelterId);
            adoptionApplicationLookup.Fields = censoredFields;

            AdoptionApplicationQuery q = adoptionApplicationLookup
                .EnrichLookup(_queryFactory)
                .Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation);

            List<Data.Entities.AdoptionApplication> datas = await q.CollectAsync();

            List<AdoptionApplication> models = await _builderFactory.Builder<AdoptionApplicationBuilder>()
                .Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
                .Build(datas, censoredFields);

            if (models == null)
                throw new NotFoundException("Adoption applications not found", JsonHelper.SerializeObjectFormatted(adoptionApplicationLookup), typeof(Data.Entities.AdoptionApplication));

            return Ok(new QueryResult<AdoptionApplication>
            {
                Items = models,
                Count = await q.CountAsync()
            });
        }

        /// <summary>
        /// Get adoptionApplication by ID.
        /// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
        /// </summary>
        [HttpGet("{id}")]
		[Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
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

            AdoptionApplication adoptionApplication = await _adoptionApplicationService.Persist(model, fields);

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
