using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Pawfect_Notifications.Data.Entities;
using Pawfect_Notifications.Data.Entities.Types.Cache;
using Pawfect_Notifications.Data.Entities.Types.Notifications;
using Pawfect_Notifications.DevTools;

namespace Pawfect_Notifications.Services.NotificationServices.Senders.InApp
{
    public class InAppSender : IInAppSender
    {
        private readonly ILogger<InAppSender> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly CacheConfig _cacheConfig;
        private readonly NotificationTemplates _templates;

        public InAppSender
        (
            ILogger<InAppSender> logger,
            IOptions<NotificationTemplates> templateOptions,
            IMemoryCache memoryCache,
            IOptions<CacheConfig> cacheOptions
        )
        {
            this._logger = logger;
            this._memoryCache = memoryCache;
            this._cacheConfig = cacheOptions.Value;
            this._templates = templateOptions.Value;
        }

        private const String cacheKeyPrefix = "email_templates_{templateId}";
        public async Task<Boolean> SendAsync(Notification notification, IServiceScope serviceScope, IClientSession session)
        {
            NotificationTemplate notificationTemplate = _templates.Templates.Find(template => template.TemplateId == notification.TeplateId);
            if (notificationTemplate == null) throw new ArgumentException("Invalid Notification Template Id");

            // Title on [0] , Content on [1]
            String[] templates = await this.GetOrAddCachedTemplates(notificationTemplate);

            // Replace placeholders
            foreach (KeyValuePair<String, String> kv in notification.TitleMappings)
                templates[0] = templates[0].Replace(kv.Key, kv.Value);

            foreach (KeyValuePair<String, String> kv in notification.ContentMappings)
                templates[1] = templates[1].Replace(kv.Key, kv.Value);

            notification.Title = templates[0];
            notification.Content = templates[1];

            return true;
        }

        private async Task<String[]> GetOrAddCachedTemplates(NotificationTemplate template)
        {
            String[] templates = null;
            String cacheKey = cacheKeyPrefix.Replace("{templateId}", template.TemplateId.ToString());

            if (_memoryCache.TryGetValue(cacheKey, out String templatesData))
            {
                templates = JsonHelper.DeserializeObjectFormattedSafe<String[]>(templatesData);
                if (templates != null && templates.Length == 2)
                    return templates;
            }

            templates = await Task.WhenAll(
                System.IO.File.ReadAllTextAsync(template.TitlePath),
                System.IO.File.ReadAllTextAsync(template.ContentPath)
            );

            _memoryCache.Set(cacheKey, JsonHelper.SerializeObjectFormattedSafe(templates), TimeSpan.FromMinutes(_cacheConfig.TemplatesCacheTime));

            return templates;
        }
    }
}
