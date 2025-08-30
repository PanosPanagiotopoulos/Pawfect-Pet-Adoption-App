using Microsoft.Extensions.Options;
using Pawfect_Notifications.Data.Entities;
using Pawfect_Notifications.Data.Entities.Types.Apis;
using Pawfect_Notifications.Data.Entities.Types.Notifications;
using SendGrid.Helpers.Mail;
using SendGrid;
using MongoDB.Driver;
using Pawfect_Notifications.Query;
using Pawfect_Notifications.Query.Queries;
using Pawfect_Notifications.Exceptions;
using Pawfect_Notifications.DevTools;
using Microsoft.Extensions.Caching.Memory;
using Pawfect_Notifications.Data.Entities.Types.Cache;
using EllipticCurve;

namespace Pawfect_Notifications.Services.NotificationServices.Senders.Email
{
    public class EmailSender : IEmailSender
    {
        private readonly ILogger<EmailSender> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly CacheConfig _cacheConfig;
        private readonly EmailApiConfig _emailConfig;
        private readonly NotificationTemplates _templates;

        public EmailSender
        (
            ILogger<EmailSender> logger,
            IOptions<EmailApiConfig> emailOptions,
            IOptions<NotificationTemplates> templateOptions,
            IMemoryCache memoryCache,
            IOptions<CacheConfig> cacheOptions
        )
        {
            this._logger = logger;
            this._memoryCache = memoryCache;
            this._cacheConfig = cacheOptions.Value;
            this._emailConfig = emailOptions.Value;
            this._templates = templateOptions.Value;
        }

        private const String cacheKey = "email_templates";
        public async Task<Boolean> SendAsync(Notification notification, IServiceScope serviceScope, IClientSession session)
        {
            ArgumentNullException.ThrowIfNull(_emailConfig);
            ArgumentNullException.ThrowIfNull(_emailConfig.ApiKey);
            ArgumentNullException.ThrowIfNull(_emailConfig.FromName);
            ArgumentNullException.ThrowIfNull(_emailConfig.FromEmail);

            // Get Templates
            NotificationTemplate notificationTemplate = _templates.Templates.Find(template => template.TemplateId == notification.TeplateId);
            if (notificationTemplate == null) throw new ArgumentException("Invalid Notification Template Id");

            // Title on [0] , Content on [1]
            String[] templates = await this.GetOrAddCachedTemplates(notificationTemplate);
                            
            // Replace placeholders
            foreach (KeyValuePair<String, String> kv in notification.TitleMappings)
                templates[0] = templates[0].Replace(kv.Key, kv.Value);

            foreach (KeyValuePair<String, String> kv in notification.ContentMappings)
                templates[1] = templates[1].Replace(kv.Key, kv.Value);

            // Fetch users email
            UserQuery userQuery = serviceScope.ServiceProvider.GetRequiredService<IQueryFactory>().Query<UserQuery>();
            userQuery.Ids = [notification.UserId];
            userQuery.Fields = userQuery.FieldNamesOf([nameof(Models.User.User.Email)]);
            userQuery.Offset = 0;
            userQuery.PageSize = 1;

            User user = (await userQuery.CollectAsync()).FirstOrDefault();
            if (user == null) throw new NotFoundException("User to send email not found");

            SendGridClient client = new SendGridClient(_emailConfig.ApiKey);
            EmailAddress from = new EmailAddress(_emailConfig.FromEmail, _emailConfig.FromName);
            EmailAddress to = new EmailAddress(user.Email);
            SendGridMessage msg = MailHelper.CreateSingleEmail(from, to, templates[0], null, templates[1]);

            Response response = await client.SendEmailAsync(msg);

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"Failed to send email\nBody : {JsonHelper.SerializeObjectFormattedSafe(await response.Body.ReadAsStringAsync())}");

            return true;
        }

        private async Task<String[]> GetOrAddCachedTemplates(NotificationTemplate template)
        {
            String[] templates = null;
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
