using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Pawfect_Notifications.Data.Entities;
using Pawfect_Notifications.Data.Entities.Types.Apis;
using Pawfect_Notifications.Data.Entities.Types.Notifications;
using Pawfect_Notifications.Query.Queries;
using Pawfect_Notifications.Query;
using Pawfect_Notifications.Exceptions;
using Vonage.Request;
using Vonage;

namespace Pawfect_Notifications.Services.NotificationServices.Senders.Sms
{
    public class SmsSender : ISmsSender
    {
        private readonly ILogger<SmsSender> _logger;
        private readonly SmsApiConfig _smsConfig;
        private readonly NotificationTemplates _templates;

        public SmsSender
        (
            ILogger<SmsSender> logger,
            IOptions<SmsApiConfig> smsOptions,
            IOptions<NotificationTemplates> templateOptions
        )
        {
            this._logger = logger;
            this._smsConfig = smsOptions.Value;
            this._templates = templateOptions.Value;
        }
        public async Task<Boolean> SendAsync(Notification notification, IServiceScope serviceScope, IClientSession session)
        {
            ArgumentNullException.ThrowIfNull(_smsConfig);
            ArgumentNullException.ThrowIfNull(_smsConfig.ApiKey);
            ArgumentNullException.ThrowIfNull(_smsConfig.ApiSecret);
            ArgumentNullException.ThrowIfNull(_smsConfig.From);

            // Get Templates
            NotificationTemplate notificationTemplate = _templates.Templates.Find(template => template.TemplateId == notification.TeplateId);
            if (notificationTemplate == null) throw new ArgumentException("Invalid Notification Template Id");

            String titleFullPath = Path.Combine(AppContext.BaseDirectory, notificationTemplate.TitlePath);
            String contentFullPath = Path.Combine(AppContext.BaseDirectory, notificationTemplate.ContentPath);

            // Content on [0]
            String[] templates = await Task.WhenAll(
                System.IO.File.ReadAllTextAsync(contentFullPath)
            );

            foreach (KeyValuePair<String, String> kv in notification.ContentMappings)
                templates[1] = templates[1].Replace(kv.Key, kv.Value);

            // Fetch users email
            UserQuery userQuery = serviceScope.ServiceProvider.GetRequiredService<QueryFactory>().Query<UserQuery>();
            userQuery.Ids = [notification.UserId];
            userQuery.Fields = userQuery.FieldNamesOf([nameof(Data.Entities.User.Phone)]);
            userQuery.Offset = 0;
            userQuery.PageSize = 1;

            User user = (await userQuery.CollectAsync()).FirstOrDefault();
            if (user == null) throw new NotFoundException("User to send sms not found");

            String cleanedPhonenumber = ISmsSender.ParsePhoneNumber(user.Phone);

            Credentials credentials = Credentials.FromApiKeyAndSecret(_smsConfig.ApiKey, _smsConfig.ApiSecret);
            VonageClient client = new VonageClient(credentials);

            Vonage.Messaging.SendSmsResponse response = await client.SmsClient.SendAnSmsAsync(new Vonage.Messaging.SendSmsRequest()
            {
                To = cleanedPhonenumber,
                From = _smsConfig.From,
                Text = templates[0]
            });

            // Check response
            if (response.Messages == null || response.Messages == null || response.Messages.Count() == 0)
                throw new InvalidOperationException($"No response received from SMS service for number: {cleanedPhonenumber}");

            Vonage.Messaging.SmsResponseMessage messageResponse = response.Messages[0];
            if (messageResponse.Status != "0")
                throw new InvalidOperationException($"Failed to send SMS to {cleanedPhonenumber}. Status: {messageResponse.Status}, Error: {messageResponse.ErrorText ?? "Unknown error"}");

            return true;
        }
    }
}
