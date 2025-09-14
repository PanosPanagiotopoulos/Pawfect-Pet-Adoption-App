using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawfect_Messenger.Attributes;
using Pawfect_Messenger.Builders;
using Pawfect_Messenger.Censors;
using Pawfect_Messenger.Data.Entities.EnumTypes;
using Pawfect_Messenger.Data.Entities.Types.Authorisation;
using Pawfect_Messenger.DevTools;
using Pawfect_Messenger.Exceptions;
using Pawfect_Messenger.Models.Message;
using Pawfect_Messenger.Models.Lookups;
using Pawfect_Messenger.Query;
using Pawfect_Messenger.Query.Queries;
using Pawfect_Messenger.Services.AuthenticationServices;
using Pawfect_Messenger.Services.Convention;
using Pawfect_Messenger.Services.MessageServices;
using Pawfect_Messenger.Transactions;
using System.Security.Claims;

namespace Pawfect_Messenger.Controllers
{
    [ApiController]
    [Route("api/messages")]
    [RateLimit(RateLimitLevel.Moderate)]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly ILogger<MessageController> _logger;
        private readonly IAuthorizationContentResolver _authorizationContentResolver;
        private readonly IBuilderFactory _builderFactory;
        private readonly ICensorFactory _censorFactory;
        private readonly IQueryFactory _queryFactory;
        private readonly AuthContextBuilder _contextBuilder;
        private readonly ClaimsExtractor _claimsExtractor;
        private readonly IConventionService _conventionService;

        public MessageController(
            IMessageService messageService,
            ILogger<MessageController> logger,
            IAuthorizationContentResolver authorizationContentResolver,
            IBuilderFactory builderFactory,
            ICensorFactory censorFactory,
            IQueryFactory queryFactory,
            AuthContextBuilder contextBuilder,
            ClaimsExtractor claimsExtractor,
            IConventionService conventionService

            )
        {
            _messageService = messageService;
            _logger = logger;
            _authorizationContentResolver = authorizationContentResolver;
            _builderFactory = builderFactory;
            _censorFactory = censorFactory;
            _queryFactory = queryFactory;
            _contextBuilder = contextBuilder;
            _claimsExtractor = claimsExtractor;
            _conventionService = conventionService;
        }

        [HttpPost("query/{conversationId}")]
        [Authorize]
        public async Task<IActionResult> QueryConversation([FromBody] MessageLookup lookup, [FromRoute] String conversationId)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (String.IsNullOrWhiteSpace(conversationId)) return BadRequest("ConversationId is required");

            ClaimsPrincipal currentUser = _authorizationContentResolver.CurrentPrincipal();
            String userId = _claimsExtractor.CurrentUserId(currentUser);
            if (!_conventionService.IsValidId(userId)) throw new UnAuthenticatedException("User is not authenticated.");

            lookup.ConversationIds = [conversationId];
            lookup.Statuses = [MessageStatus.Sending, MessageStatus.Delivered];

            AuthContext context = _contextBuilder.OwnedFrom(lookup, userId).AffiliatedWith(lookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<MessageCensor>().Censor([.. lookup.Fields], context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying messages");

            lookup.Fields = censoredFields;
            MessageQuery q = lookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation);

            List<Data.Entities.Message> datas = await q.CollectAsync();

            List<Message> models = await _builderFactory.Builder<MessageBuilder>().Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation).Build(datas, censoredFields);

            if (models == null) throw new NotFoundException("Messages not found", JsonHelper.SerializeObjectFormatted(lookup), typeof(Data.Entities.Message));

            return Ok(new QueryResult<Message>
            {
                Items = models,
                Count = await q.CountAsync()
            });
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetMessage(String id, [FromQuery] List<String> fields)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            MessageLookup lookup = new MessageLookup();
            // Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
            lookup.Offset = 0;
            // Γενική τιμή για τη λήψη των dtos
            lookup.PageSize = 1;
            lookup.Ids = [id];
            lookup.Statuses = [MessageStatus.Sending, MessageStatus.Delivered];

            AuthContext context = _contextBuilder.OwnedFrom(lookup).AffiliatedWith(lookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<MessageCensor>().Censor(BaseCensor.PrepareFieldsList(fields), context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying messages");

            lookup.Fields = censoredFields;
            Message model = (
                            await _builderFactory.Builder<MessageBuilder>().Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
                            .Build(await lookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation).CollectAsync(), censoredFields)
                            )
                            .FirstOrDefault();

            if (model == null) throw new NotFoundException("Message not found", JsonHelper.SerializeObjectFormatted(lookup), typeof(Data.Entities.Message));

            return Ok(model);
        }

        [HttpPost("persist")]
        [Authorize]
        [RateLimit(RateLimitLevel.Moderate)]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> Persist([FromBody] MessagePersist model, [FromQuery] List<String> fields)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            fields = BaseCensor.PrepareFieldsList(fields);

            Message message = await _messageService.Persist(model, fields);

            return Ok(message);
        }

        [HttpPost("read")]
        [Authorize]
        [RateLimit(RateLimitLevel.Moderate)]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> MarkRead([FromBody] List<MessageReadPersist> models, [FromQuery] List<String> fields)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            fields = BaseCensor.PrepareFieldsList(fields);
            
            await _messageService.MarkRead(models, fields);
            
            return Ok();
        }


        [HttpPost("delete/{id}")]
        [Authorize]
        [RateLimit(RateLimitLevel.Restrictive)]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> Delete([FromRoute] String id)
        {
            if (String.IsNullOrEmpty(id) || !ModelState.IsValid) return BadRequest(ModelState);

            await _messageService.Delete(id);

            return Ok();
        }
    }
}
