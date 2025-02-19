using System.Security.Claims;

namespace Pawfect_Pet_Adoption_App_API.Services.HttpServices
{
	public class RequestService
	{
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly ILogger<RequestService> _logger;

		public RequestService(IHttpContextAccessor httpContextAccessor, ILogger<RequestService> logger)
		{
			_httpContextAccessor = httpContextAccessor;
			_logger = logger;
		}

		// Επιστροφή του Base URI του API
		public String? GetBaseURI()
		{
			try
			{
				HttpRequest? request = _httpContextAccessor?.HttpContext?.Request;
				if (request == null)
				{
					throw new InvalidOperationException("Δεν βρέθηκε το Request απο τον Accessor");
				}

				return $"{request.Scheme}://{request.Host}{request.PathBase}";
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error προσπαθώντας να βρεθεί το URI του request");
				return null;
			}
		}

		// Επιστροφή του Authenticated User ID απο το request
		public String? GetUserId()
		{
			try
			{
				return _httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error προσπαθώντας να βρεθεί το id του Authenticated χρήστη");
				return null;
			}

		}
	}
}
