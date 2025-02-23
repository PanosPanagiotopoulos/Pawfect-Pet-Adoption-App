using Microsoft.AspNetCore.Mvc;

using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Notification;
using Pawfect_Pet_Adoption_App_API.Services.NotificationServices;

namespace Pawfect_Pet_Adoption_App_API.Controllers
{
	[ApiController]
	[Route("api/notifications")]
	public class NotificationController : ControllerBase
	{
		private readonly INotificationService _notificationService;
		private readonly ILogger<NotificationController> _logger;

		public NotificationController(INotificationService notificationService, ILogger<NotificationController> logger)
		{
			_notificationService = notificationService;
			_logger = logger;
		}

		/// <summary>
		/// Query notifications.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpGet("query")]
		[ProducesResponseType(200, Type = typeof(IEnumerable<NotificationDto>))]
		[ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
		[ProducesResponseType(404)]
		[ProducesResponseType(500, Type = typeof(String))]
		public async Task<IActionResult> QueryNotifications([FromQuery] NotificationLookup notificationLookup)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				IEnumerable<NotificationDto>? notifications = await _notificationService.QueryNotificationsAsync(notificationLookup);

				if (notifications == null)
				{
					return NotFound();
				}

				return Ok(notifications);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error ενώ κάναμε query notifications");
				return RequestHandlerTool.HandleInternalServerError(e, "GET");
			}
		}

		/// <summary>
		/// Get notification by ID.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
		/// </summary>
		[HttpGet("{id}")]
		[ProducesResponseType(200, Type = typeof(NotificationDto))]
		[ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
		[ProducesResponseType(404)]
		[ProducesResponseType(500, Type = typeof(String))]
		public async Task<IActionResult> GetNotification([FromRoute] String id, [FromQuery] List<String> fields)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				NotificationDto? notification = await _notificationService.Get(id, fields);

				if (notification == null)
				{
					return NotFound();
				}

				return Ok(notification);
			}
			catch (InvalidDataException e)
			{
				_logger.LogError(e, "Δεν βρέθηκε ειδοποίηση");
				return NotFound();
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error ενώ κάναμε query notification");
				return RequestHandlerTool.HandleInternalServerError(e, "GET");
			}
		}

		/// <summary>
		/// Persist a notification.
		/// </summary>
		[HttpPost("persist")]
		[ProducesResponseType(200, Type = typeof(NotificationDto))]
		[ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
		[ProducesResponseType(500, Type = typeof(String))]
		public async Task<IActionResult> Persist([FromBody] NotificationPersist model)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				NotificationDto? notification = await _notificationService.Persist(model);

				if (notification == null)
				{
					return RequestHandlerTool.HandleInternalServerError(new Exception("Failed to save model. Null return"), "POST");
				}

				return Ok(notification);
			}
			catch (InvalidOperationException e)
			{
				_logger.LogError(e, "Αποτυχία αποθήκευσης ειδοποίησης");
				return NotFound();
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error ενώ κάναμε persist notification");
				return RequestHandlerTool.HandleInternalServerError(e, "POST");
			}
		}
	}
}
