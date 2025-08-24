using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawfect_Notifications.Builders;
using Pawfect_Notifications.Censors;
using Pawfect_Notifications.Data.Entities.Types.Authorization;
using Pawfect_Notifications.DevTools;
using Pawfect_Notifications.Exceptions;
using Pawfect_Notifications.Models.Lookups;
using Pawfect_Notifications.Models.Notification;
using Pawfect_Notifications.Query;
using Pawfect_Notifications.Services.NotificationServices;
using Pawfect_Notifications.Transactions;
using Pawfect_Notifications.Query.Queries;
using Pawfect_Notifications.Services.AuthenticationServices;
using System.Security.Claims;
using Pawfect_Notifications.Services.Convention;
using Pawfect_Notifications.Attributes;
using Pawfect_Notifications.Data.Entities.EnumTypes;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Pawfect_Notifications.Controllers
{
	[ApiController]
	[Route("api/notifications")]
	public class NotificationController : ControllerBase
	{
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationController> _logger;
        private readonly IBuilderFactory _builderFactory;
        private readonly ICensorFactory _censorFactory ;
        private readonly AuthContextBuilder _contextBuilder;
        private readonly IAuthorizationContentResolver _authorizationContentResolver;
        private readonly ClaimsExtractor _claimsExtractor;
        private readonly IConventionService _conventionService;
        private readonly IQueryFactory _queryFactory;

        public NotificationController(
            INotificationService notificationService,
            ILogger<NotificationController> logger,
            IBuilderFactory builderFactory,
			ICensorFactory censorFactory,
            AuthContextBuilder contextBuilder,
            IAuthorizationContentResolver authorizationContentResolver,
            ClaimsExtractor claimsExtractor,
            IConventionService conventionService,
            IQueryFactory queryFactory)
        {
            _notificationService = notificationService;
            _logger = logger;
            _builderFactory = builderFactory;
            _censorFactory = censorFactory;
            _contextBuilder = contextBuilder;
            _authorizationContentResolver = authorizationContentResolver;
            _claimsExtractor = claimsExtractor;
            _conventionService = conventionService;
            _queryFactory = queryFactory;
        }

        [HttpPost("query/mine/unread")]
		[Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> QueryNotifications([FromBody] NotificationLookup notificationLookup)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

            ClaimsPrincipal currentUser = _authorizationContentResolver.CurrentPrincipal();
            String userId = _claimsExtractor.CurrentUserId(currentUser);
            if (!_conventionService.IsValidId(userId)) throw new UnAuthenticatedException("User is not authenticated.");

            notificationLookup.UserIds = [userId];
            notificationLookup.IsRead = false;
            notificationLookup.NotificationTypes = [NotificationType.InApp];

            NotificationQuery q = notificationLookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermission);
            List<Data.Entities.Notification> datas = await q.CollectAsync();

            List<Notification> models = await _builderFactory.Builder<NotificationBuilder>()
                .Authorise(AuthorizationFlags.OwnerOrPermission)
                .Build(datas, [.. notificationLookup.Fields]);

            if (models == null) throw new NotFoundException("Notifications not found", JsonHelper.SerializeObjectFormatted(notificationLookup), typeof(Data.Entities.Notification));

            return Ok(new QueryResult<Notification>()
            {
                Items = models,
                Count = await q.CountAsync()
            });
		}

        [HttpPost("read")]
        [Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> ReadNotifications([FromBody] List<String> notificationIds, [FromQuery] List<String> fields)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (notificationIds == null || !notificationIds.Any()) return BadRequest("At least one notification ID is required");

            fields = BaseCensor.PrepareFieldsList(fields);

            List<Models.Notification.Notification> readNotifications = await _notificationService.ReadNotificationsAsync(notificationIds, fields);

            if (readNotifications.Count != notificationIds.Count)
            {
                _logger.LogError("Not all notifications were read successfully. Expected: {ExpectedCount}, Actual: {ActualCount}", notificationIds.Count, readNotifications.Count);
                return StatusCode(500, "Not all notifications were read successfully. Please try again later.");
            }

            return Ok(readNotifications);
        }


        [HttpPost("persist/batch")]
        [ServiceFilter(typeof(InternalApiAttribute))]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> Persist([FromBody] List<NotificationEvent> models)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (models == null || !models.Any()) return BadRequest("At least one notification event is required");

            await _notificationService.HandleEvents(models);

            return Ok();
        }

        [HttpPost("delete/{id}")]
        [Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> Delete([FromRoute] String id)
        {
            if (String.IsNullOrEmpty(id) || !ModelState.IsValid) return BadRequest(ModelState);

            await _notificationService.Delete(id);

            return Ok();
        }
    }
}
