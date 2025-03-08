using Microsoft.Extensions.Options;

using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authentication;

using System.Security.Claims;

namespace Pawfect_Pet_Adoption_App_API.Services.HttpServices
{
	public class RequestService
	{
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly ILogger<RequestService> _logger;
		private readonly CorsConfig _corsConfig;
		public RequestService(
			IHttpContextAccessor httpContextAccessor,
			ILogger<RequestService> logger,
			IOptions<CorsConfig> corsConfig
			)
		{
			_httpContextAccessor = httpContextAccessor;
			_logger = logger;
			_corsConfig = corsConfig.Value;
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

		// Επιστροφή του Base URI του API
		public String? GetFrontendBaseURI()
		{
			try
			{
				String? baseFrontendUri = _corsConfig.AllowedOrigins[0];
				if (String.IsNullOrEmpty(baseFrontendUri))
				{
					throw new InvalidOperationException("Δεν βρέθηκε το Base URI του Frontend");
				}

				return $"{baseFrontendUri}/";
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error προσπαθώντας να βρεθεί το URI για το frontend");
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
