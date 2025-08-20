using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Pawfect_Notifications.Data.Entities;
using Pawfect_Notifications.Data.Entities.Types.Notifications;

namespace Pawfect_Notifications.Services.NotificationServices.Senders.InApp
{
    public class InAppSender : IInAppSender
    {
        private readonly ILogger<InAppSender> _logger;
        private readonly NotificationTemplates _templates;

        public InAppSender
        (
            ILogger<InAppSender> logger,
            IOptions<NotificationTemplates> templateOptions
        )
        {
            this._logger = logger;
            this._templates = templateOptions.Value;
        }
        public async Task<Boolean> SendAsync(Notification notification, IServiceScope serviceScope, IClientSession session)
        {
            NotificationTemplate notificationTemplate = _templates.Templates.Find(template => template.TemplateId == notification.TeplateId);
            if (notificationTemplate == null) throw new ArgumentException("Invalid Notification Template Id");

            String titleFullPath = Path.Combine(AppContext.BaseDirectory, notificationTemplate.TitlePath);
            String contentFullPath = Path.Combine(AppContext.BaseDirectory, notificationTemplate.ContentPath);

            // Title on [0] , Content on [1]
            String[] templates = await Task.WhenAll(
                System.IO.File.ReadAllTextAsync(titleFullPath),
                System.IO.File.ReadAllTextAsync(contentFullPath)
            );

            // Replace placeholders
            foreach (KeyValuePair<String, String> kv in notification.TitleMappings)
                templates[0] = templates[0].Replace(kv.Key, kv.Value);

            foreach (KeyValuePair<String, String> kv in notification.ContentMappings)
                templates[1] = templates[1].Replace(kv.Key, kv.Value);

            notification.Title = templates[0];
            notification.Content = templates[1];

            return true;
        }
    }
}
