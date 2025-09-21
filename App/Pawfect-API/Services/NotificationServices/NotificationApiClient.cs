using Microsoft.Extensions.Options;
using Pawfect_API.DevTools;
using Pawfect_API.Models.Notification;
using Pawfect_API.Data.Entities.Types.Apis;
using System.Security.Cryptography;
using System.Text;

namespace Pawfect_API.Services.NotificationServices
{
    public class NotificationApiClient : INotificationApiClient
    {
        private readonly NotificationApiConfig _config;
        private readonly ILogger<NotificationApiClient> _logger;

        public NotificationApiClient
        (
            ILogger<NotificationApiClient> logger,
            IOptions<NotificationApiConfig> options
        )
        {
            this._config = options.Value;
            this._logger = logger;
        }
        public async Task NotificationEvent(NotificationEvent notificationEvent) => await this.NotificationEvent([notificationEvent]);
        public async Task NotificationEvent(List<NotificationEvent> notificationEvents)
        {
            String timestamp = DateTime.UtcNow.ToString();

            String signature = GenerateSignature(_config.FromServiceName, timestamp, _config.SharedSecret);

            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _config.NotificationEventUrl);

                // Add internal API headers
                request.Headers.Add("X-Internal-Service", _config.FromServiceName);
                request.Headers.Add("X-Internal-Signature", signature);
                request.Headers.Add("X-Internal-Timestamp", timestamp);

                // Add content
                request.Content = JsonContent.Create(notificationEvents);

                using (HttpClient client = new HttpClient())
                {
                    using HttpResponseMessage response = await client.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogError("Failed to send notification: {StatusCode} - {Content}",
                        response.StatusCode, await response.Content.ReadAsStringAsync());
                    }
                }
            } catch (Exception ex)
            {
                _logger.LogError($"Failed to send notifications request with data\n{ex.Message}\n{JsonHelper.SerializeObjectFormattedSafe(ex.Data)}\n{ex.StackTrace}{JsonHelper.SerializeObjectFormattedSafe(notificationEvents)}");
            }
        }

        private String GenerateSignature(String serviceName, String timestamp, String secret)
        {
            String payload = $"{serviceName}:{timestamp}:{secret}";
            
            using HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            
            Byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            
            return Convert.ToBase64String(hash);
        }
    }
}