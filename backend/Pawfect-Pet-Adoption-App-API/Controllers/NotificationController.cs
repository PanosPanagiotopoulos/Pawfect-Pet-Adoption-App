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
		[HttpPost("query")]
		[ProducesResponseType(200, Type = typeof(IEnumerable<NotificationDto>))]
		[ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
		[ProducesResponseType(404)]
		[ProducesResponseType(500, Type = typeof(String))]
		public async Task<IActionResult> QueryNotifications([FromBody] NotificationLookup notificationLookup)
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
		public async Task<IActionResult> GetNotification(String id, [FromQuery] List<String> fields)
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
		public async Task<IActionResult> Persist([FromBody] NotificationPersist model, [FromQuery] List<String> fields)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				NotificationDto? notification = await _notificationService.Persist(model, fields);

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

		/// <summary>
		/// Delete a notification by ID.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 404 NotFound, 500 String
		/// </summary>
		[HttpPost("delete")]
		[ProducesResponseType(200)]
		[ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
		[ProducesResponseType(404)]
		[ProducesResponseType(500, Type = typeof(String))]
		public async Task<IActionResult> Delete([FromBody] String id)
		{
			// TODO: Add authorization
			if (String.IsNullOrEmpty(id) || !ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				await _notificationService.Delete(id);
				return Ok();
			}
			catch (InvalidOperationException e)
			{
				_logger.LogError(e, "Αποτυχία διαγραφής ειδοποίησης με ID {Id}", id);
				return NotFound();
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error ενώ κάναμε delete notification με ID {Id}", id);
				return RequestHandlerTool.HandleInternalServerError(e, "POST");
			}
		}

		/// <summary>
		/// Delete multiple notifications by IDs.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 404 NotFound, 500 String
		/// </summary>
		[HttpPost("delete/many")]
		[ProducesResponseType(200)]
		[ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
		[ProducesResponseType(404)]
		[ProducesResponseType(500, Type = typeof(String))]
		public async Task<IActionResult> DeleteMany([FromBody] List<String> ids)
		{
			// TODO: Add authorization
			if (ids == null || !ids.Any() || !ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				await _notificationService.Delete(ids);
				return Ok();
			}
			catch (InvalidOperationException e)
			{
				_logger.LogError(e, "Αποτυχία διαγραφής ειδοποιήσεων με IDs {Ids}", String.Join(", ", ids));
				return NotFound();
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error ενώ κάναμε delete πολλαπλών notifications με IDs {Ids}", String.Join(", ", ids));
				return RequestHandlerTool.HandleInternalServerError(e, "POST");
			}
		}
	}
}
