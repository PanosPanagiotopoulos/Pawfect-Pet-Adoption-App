using Pawfect_API.Builders;
using Pawfect_API.Censors;
using Pawfect_API.Data.Entities.Types.Authorization;
using Pawfect_API.DevTools;
using Pawfect_API.Models.AnimalType;
using Pawfect_API.Models.Lookups;
using Pawfect_API.Query.Queries;
using Pawfect_API.Query;
using Pawfect_API.Transactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawfect_Pet_Adoption_App_API.Query;
using Pawfect_API.Services.AnimalTypeServices;
using Pawfect_API.Exceptions;

namespace Pawfect_Pet_Adoption_App_API.Controllers
{
    [ApiController]
    [Route("api/animal-types")]
    public class AnimalTypeController : ControllerBase
    {
        private readonly IAnimalTypeService _animalTypeService;
        private readonly ILogger<AnimalTypeController> _logger;
        private readonly IBuilderFactory _builderFactory;
        private readonly ICensorFactory _censorFactory;
        private readonly AuthContextBuilder _contextBuilder;
        private readonly IQueryFactory _queryFactory;

        public AnimalTypeController
        (
            IAnimalTypeService animalTypeService,
            ILogger<AnimalTypeController> logger,
            IBuilderFactory builderFactory,
            ICensorFactory censorFactory,
            AuthContextBuilder contextBuilder,
            IQueryFactory queryFactory
        )
        {
            _animalTypeService = animalTypeService;
            _logger = logger;
            _builderFactory = builderFactory;
            _censorFactory = censorFactory;
            _contextBuilder = contextBuilder;
            _queryFactory = queryFactory;
        }

        [HttpPost("query")]
        [Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> QueryAnimalTypes([FromBody] AnimalTypeLookup animalTypeLookup)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            AuthContext context = _contextBuilder.OwnedFrom(animalTypeLookup).AffiliatedWith(animalTypeLookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<AnimalTypeCensor>().Censor([.. animalTypeLookup.Fields], context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying AnimalTypes");

            animalTypeLookup.Fields = censoredFields;
            AnimalTypeQuery q = animalTypeLookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation);

            List<Pawfect_API.Data.Entities.AnimalType> datas = await q.CollectAsync();

            List<AnimalType> models = await _builderFactory.Builder<AnimalTypeBuilder>()
                .Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
                .Build(datas, [.. animalTypeLookup.Fields]);

            if (models == null) throw new NotFoundException("AnimalTypes not found", JsonHelper.SerializeObjectFormatted(animalTypeLookup), typeof(Pawfect_API.Data.Entities.AnimalType));


            return Ok(new QueryResult<AnimalType>()
            {
                Items = models,
                Count = await q.CountAsync()
            });
        }

        [HttpGet("{id}")]
        [Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> GetAnimalType(String id, [FromQuery] List<String> fields)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            AnimalTypeLookup lookup = new AnimalTypeLookup
            {
                Offset = 1,
                PageSize = 1,
                Ids = new List<String> { id },
                Fields = fields
            };

            AuthContext context = _contextBuilder.OwnedFrom(lookup).AffiliatedWith(lookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<AnimalTypeCensor>().Censor(BaseCensor.PrepareFieldsList([.. lookup.Fields]), context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying AnimalTypes");

            lookup.Fields = censoredFields;
            AnimalType model = (await _builderFactory.Builder<AnimalTypeBuilder>().Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
                .Build(await lookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation).CollectAsync(), censoredFields))
            .FirstOrDefault();

            if (model == null) throw new NotFoundException("AnimalType not found", JsonHelper.SerializeObjectFormatted(lookup), typeof(Pawfect_API.Data.Entities.AnimalType));

            return Ok(model);
        }

        [HttpPost("persist")]
        [Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> Persist([FromBody] AnimalTypePersist model, [FromQuery] List<String> fields)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            fields = BaseCensor.PrepareFieldsList(fields);

            AnimalType animalType = await _animalTypeService.Persist(model, fields);

            return Ok(animalType);
        }

        [HttpPost("delete/{id}")]
        [Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> Delete([FromRoute] String id)
        {
            if (String.IsNullOrEmpty(id) || !ModelState.IsValid) return BadRequest(ModelState);

            await _animalTypeService.Delete(id);

            return Ok();
        }
    }
}
