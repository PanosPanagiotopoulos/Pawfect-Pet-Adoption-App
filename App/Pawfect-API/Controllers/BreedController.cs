using Pawfect_API.Builders;
using Pawfect_API.Censors;
using Pawfect_API.Data.Entities.Types.Authorization;
using Pawfect_API.DevTools;
using Pawfect_API.Models.Breed;
using Pawfect_API.Models.Lookups;
using Pawfect_API.Query.Queries;
using Pawfect_API.Query;
using Pawfect_API.Transactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawfect_Pet_Adoption_App_API.Query;
using Pawfect_API.Exceptions;
using Pawfect_API.Services.BreedServices;
using Pawfect_Pet_Adoption_App_API.Attributes;
using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;

namespace Pawfect_Pet_Adoption_App_API.Controllers
{
    [ApiController]
    [Route("api/breeds")]
    [RateLimit(RateLimitLevel.Moderate)]
    public class BreedController : ControllerBase
    {
        private readonly IBreedService _breedService;
        private readonly ILogger<BreedController> _logger;
        private readonly IBuilderFactory _builderFactory;
        private readonly ICensorFactory _censorFactory;
        private readonly AuthContextBuilder _contextBuilder;
        private readonly IQueryFactory _queryFactory;

        public BreedController
        (
            IBreedService breedService,
            ILogger<BreedController> logger,
            IBuilderFactory builderFactory,
            ICensorFactory censorFactory,
            AuthContextBuilder contextBuilder,
            IQueryFactory queryFactory
        )
        {
            _breedService = breedService;
            _logger = logger;
            _builderFactory = builderFactory;
            _censorFactory = censorFactory;
            _contextBuilder = contextBuilder;
            _queryFactory = queryFactory;
        }
        
        [HttpPost("query")]
        [Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> QueryBreeds([FromBody] BreedLookup breedLookup)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            AuthContext context = _contextBuilder.OwnedFrom(breedLookup).AffiliatedWith(breedLookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<BreedCensor>().Censor([.. breedLookup.Fields], context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying Breeds");

            breedLookup.Fields = censoredFields;
            BreedQuery q = breedLookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation);

            List<Pawfect_API.Data.Entities.Breed> datas = await q.CollectAsync();
                
            List<Breed> models = await _builderFactory.Builder<BreedBuilder>()
                .Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
                .Build(datas, [.. breedLookup.Fields]);

            if (models == null) throw new NotFoundException("Breeds not found", JsonHelper.SerializeObjectFormatted(breedLookup), typeof(Pawfect_API.Data.Entities.Breed));


            return Ok(new QueryResult<Breed>()
            {
                Items = models,
                Count = await q.CountAsync()
            });
        }

        [HttpGet("{id}")]
        [Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> GetBreed(String id, [FromQuery] List<String> fields)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            BreedLookup lookup = new BreedLookup
            {
                Offset = 0,
                PageSize = 1,
                Ids = new List<String> { id },
                Fields = fields
            };

            AuthContext context = _contextBuilder.OwnedFrom(lookup).AffiliatedWith(lookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<BreedCensor>().Censor(BaseCensor.PrepareFieldsList([.. lookup.Fields]), context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying Breeds");

            lookup.Fields = censoredFields;
            Breed model = (await _builderFactory.Builder<BreedBuilder>().Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
                .Build(await lookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation).CollectAsync(), censoredFields))
            .FirstOrDefault();

            if (model == null) throw new NotFoundException("Breed not found", JsonHelper.SerializeObjectFormatted(lookup), typeof(Pawfect_API.Data.Entities.Breed));

            return Ok(model);
        }

        [HttpPost("persist")]
        [Authorize]
        [RateLimit(RateLimitLevel.Restrictive)]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> Persist([FromBody] BreedPersist model, [FromQuery] List<String> fields)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            fields = BaseCensor.PrepareFieldsList(fields);

            Breed breed = await _breedService.Persist(model, fields);

            return Ok(breed);
        }

        [HttpPost("delete/{id}")]
        [Authorize]
        [RateLimit(RateLimitLevel.Restrictive)]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> Delete([FromRoute] String id)
        {
            if (String.IsNullOrEmpty(id) || !ModelState.IsValid) return BadRequest(ModelState);

            await _breedService.Delete(id);

            return Ok();
        }
    }
}
