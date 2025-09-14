using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawfect_Messenger.Attributes;
using Pawfect_Messenger.Builders;
using Pawfect_Messenger.Censors;
using Pawfect_Messenger.Data.Entities.EnumTypes;
using Pawfect_Messenger.Data.Entities.Types.Authorisation;
using Pawfect_Messenger.DevTools;
using Pawfect_Messenger.Exceptions;
using Pawfect_Messenger.Models.Conversation;
using Pawfect_Messenger.Models.Lookups;
using Pawfect_Messenger.Query;
using Pawfect_Messenger.Query.Queries;
using Pawfect_Messenger.Services.AuthenticationServices;
using Pawfect_Messenger.Services.Convention;
using Pawfect_Messenger.Services.ConversationServices;
using Pawfect_Messenger.Transactions;
using System.Security.Claims;

namespace Pawfect_Messenger.Controllers
{
    [ApiController]
    [Route("api/conversations")]
    [RateLimit(RateLimitLevel.Moderate)]
    public class ConversationController : ControllerBase
    {
        private readonly IConversationService _conversationService;
        private readonly ILogger<ConversationController> _logger;
        private readonly IAuthorizationContentResolver _authorizationContentResolver;
        private readonly IBuilderFactory _builderFactory;
        private readonly ICensorFactory _censorFactory;
        private readonly IQueryFactory _queryFactory;
        private readonly AuthContextBuilder _contextBuilder;
        private readonly ClaimsExtractor _claimsExtractor;
        private readonly IConventionService _conventionService;

        public ConversationController(
            IConversationService conversationService,
            ILogger<ConversationController> logger,
            IAuthorizationContentResolver authorizationContentResolver,
            IBuilderFactory builderFactory,
            ICensorFactory censorFactory,
            IQueryFactory queryFactory,
            AuthContextBuilder contextBuilder,
            ClaimsExtractor claimsExtractor,
            IConventionService conventionService

            )
        {
            _conversationService = conversationService;
            _logger = logger;
            _authorizationContentResolver = authorizationContentResolver;
            _builderFactory = builderFactory;
            _censorFactory = censorFactory;
            _queryFactory = queryFactory;
            _contextBuilder = contextBuilder;
            _claimsExtractor = claimsExtractor;
            _conventionService = conventionService;
        }

        [HttpPost("query/mine")]
        [Authorize]
        public async Task<IActionResult> QueryMine([FromBody] ConversationLookup conversationLookup)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            ClaimsPrincipal currentUser = _authorizationContentResolver.CurrentPrincipal();
            String userId = _claimsExtractor.CurrentUserId(currentUser);
            if (!_conventionService.IsValidId(userId)) throw new UnAuthenticatedException("User is not authenticated.");

            conversationLookup.Participants = [userId];

            AuthContext context = _contextBuilder.OwnedFrom(conversationLookup, userId).AffiliatedWith(conversationLookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<ConversationCensor>().Censor([.. conversationLookup.Fields], context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying conversations");

            conversationLookup.Fields = censoredFields;

            ConversationQuery q = conversationLookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation);

            List<Data.Entities.Conversation> datas = await q.CollectAsync();

            List<Conversation> models = await _builderFactory.Builder<ConversationBuilder>().Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation).Build(datas, censoredFields);

            if (models == null)
                throw new NotFoundException("Adoption applications not found", JsonHelper.SerializeObjectFormatted(conversationLookup), typeof(Data.Entities.Conversation));

            return Ok(new QueryResult<Conversation>
            {
                Items = models,
                Count = await q.CountAsync()
            });
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetConversation(String id, [FromQuery] List<String> fields)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            ConversationLookup lookup = new ConversationLookup();
            // Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
            lookup.Offset = 0;
            // Γενική τιμή για τη λήψη των dtos
            lookup.PageSize = 1;
            lookup.Ids = [id];

            AuthContext context = _contextBuilder.OwnedFrom(lookup).AffiliatedWith(lookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<ConversationCensor>().Censor(BaseCensor.PrepareFieldsList(fields), context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying adoption applications");

            lookup.Fields = censoredFields;
            Conversation model = (
                                await _builderFactory.Builder<ConversationBuilder>().Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
                                .Build(await lookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation).CollectAsync(), censoredFields)
                                )
                                .FirstOrDefault();

            if (model == null) throw new NotFoundException("Adoption applications not found", JsonHelper.SerializeObjectFormatted(lookup), typeof(Data.Entities.Conversation));

            return Ok(model);
        }

        [HttpPost("create")]
        [Authorize]
        [RateLimit(RateLimitLevel.Restrictive)]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> Create([FromBody] ConversationPersist model, [FromQuery] List<String> fields)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            fields = BaseCensor.PrepareFieldsList(fields);

            Conversation conversation = await _conversationService.CreateAsync(model, fields);

            return Ok(conversation);
        }


        [HttpPost("delete/{id}")]
        [Authorize]
        [RateLimit(RateLimitLevel.Restrictive)]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> Delete([FromRoute] String id)
        {
            if (String.IsNullOrEmpty(id) || !ModelState.IsValid) return BadRequest(ModelState);

            await _conversationService.Delete(id);

            return Ok();
        }
    }
}
