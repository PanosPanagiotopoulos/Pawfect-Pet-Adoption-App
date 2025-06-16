using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Main_API.Builders;
using Main_API.Censors;
using Main_API.Data.Entities.Types.Authorization;
using Main_API.DevTools;
using Main_API.Exceptions;
using Main_API.Models.Lookups;
using Main_API.Models.Notification;
using Main_API.Query;
using Main_API.Services.NotificationServices;
using Main_API.Transactions;
using System.Linq;
using System.Reflection;

namespace Main_API.Controllers
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
        private readonly IQueryFactory _queryFactory;

        public NotificationController(
            INotificationService notificationService,
            ILogger<NotificationController> logger,
            IBuilderFactory builderFactory,
			ICensorFactory censorFactory,
            AuthContextBuilder contextBuilder,
            IQueryFactory queryFactory)
        {
            _notificationService = notificationService;
            _logger = logger;
            _builderFactory = builderFactory;
            _censorFactory = censorFactory;
            _contextBuilder = contextBuilder;
            _queryFactory = queryFactory;
        }

        /// <summary>
        /// Query notifications.
        /// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
        /// </summary>
        [HttpPost("query")]
		[Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> QueryNotifications([FromBody] NotificationLookup notificationLookup)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

            AuthContext context = _contextBuilder.OwnedFrom(notificationLookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<NotificationCensor>().Censor([.. notificationLookup.Fields], context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying notifications");

            notificationLookup.Fields = censoredFields;
            List<Data.Entities.Notification> datas = await notificationLookup
                .EnrichLookup(_queryFactory)
                .Authorise(AuthorizationFlags.OwnerOrPermission)
                .CollectAsync();

            List<Notification> models = await _builderFactory.Builder<NotificationBuilder>()
                .Authorise(AuthorizationFlags.OwnerOrPermission)
                .Build(datas, [.. notificationLookup.Fields]);

            if (models == null) throw new NotFoundException("Notifications not found", JsonHelper.SerializeObjectFormatted(notificationLookup), typeof(Data.Entities.Notification));

            return Ok(models);
		}

		/// <summary>
		/// Get notification by ID.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpGet("{id}")]
		[Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> GetNotification(String id, [FromQuery] List<String> fields)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

            NotificationLookup lookup = new NotificationLookup
            {
                Offset = 1,
                PageSize = 1,
                Ids = new List<String> { id },
                Fields = fields
            };

            AuthContext context = _contextBuilder.OwnedFrom(lookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<NotificationCensor>().Censor(BaseCensor.PrepareFieldsList([.. lookup.Fields]), context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying notifications");

            lookup.Fields = censoredFields;
            Notification model = (await _builderFactory.Builder<NotificationBuilder>().Authorise(AuthorizationFlags.OwnerOrPermission)
                .Build(await lookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermission).CollectAsync(), censoredFields))
            .FirstOrDefault();

            if (model == null) throw new NotFoundException("Notification not found", JsonHelper.SerializeObjectFormatted(lookup), typeof(Data.Entities.Notification));

            return Ok(model);
		}

		/// <summary>
		/// Persist a notification.
		/// </summary>
		[HttpPost("persist")]
		[Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> Persist([FromBody] NotificationPersist model, [FromQuery] List<String> fields)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

            fields = BaseCensor.PrepareFieldsList(fields);

            Notification notification = await _notificationService.Persist(model, fields);

			return Ok(notification);
		}

		/// <summary>
		/// Delete a notification by ID.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 404 NotFound, 500 String
		/// </summary>
		[HttpPost("delete")]
		[Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> Delete([FromBody] String id)
		{
			// TODO: Add authorization
			if (String.IsNullOrEmpty(id) || !ModelState.IsValid) return BadRequest(ModelState);

			await _notificationService.Delete(id);

			return Ok();
		}

		/// <summary>
		/// Delete multiple notifications by IDs.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 404 NotFound, 500 String
		/// </summary>
		[HttpPost("delete/many")]
		[Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> DeleteMany([FromBody] List<String> ids)
		{
			if (ids == null || !ids.Any() || !ModelState.IsValid) return BadRequest(ModelState);

			await _notificationService.Delete(ids);

			return Ok();
		}
	}
}
