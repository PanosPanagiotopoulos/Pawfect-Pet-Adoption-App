using Pawfect_Notifications.Data.Entities.EnumTypes;
using Pawfect_Notifications.Data.Entities.Types.Apis;
using Pawfect_Notifications.Data.Entities.Types.Notifications;
using Pawfect_Notifications.Services.NotificationServices.Senders;
using Pawfect_Notifications.Services.NotificationServices.Senders.Email;
using Pawfect_Notifications.Services.NotificationServices.Senders.InApp;
using Pawfect_Notifications.Services.NotificationServices.Senders.Sms;

namespace Pawfect_Notifications.Services.NotificationServices.Extention
{
	public static class Extention
	{
		public static IServiceCollection AddNotificationServices(
			this IServiceCollection services, 
			IConfiguration notificationConfig,
            IConfiguration notificationTemplatesConfig,
            IConfiguration emailApiConfig,
            IConfiguration smsApiConfig

        )
		{
			services.Configure<NotificationConfig>(notificationConfig);
            services.Configure<NotificationTemplates>(notificationTemplatesConfig);
            services.Configure<EmailApiConfig>(emailApiConfig);
            services.Configure<SmsApiConfig>(smsApiConfig);

            services.AddScoped<INotificationService, NotificationService>();
			services.AddScoped(provider => new Lazy<INotificationService>(() => provider.GetRequiredService<INotificationService>()));

            services.AddTransient<NotificationSenderFactory>();

            services.AddTransient<Pawfect_Notifications.Services.NotificationServices.Senders.Email.IEmailSender, EmailSender>();
            services.AddTransient<ISmsSender, SmsSender>();
            services.AddTransient<IInAppSender, InAppSender>();

            services.Configure<NotificationSenderFactory.NotificationSenderFactoryConfig>(x =>
            {
                // -- Email --
                x.Add(NotificationType.Email, typeof(Pawfect_Notifications.Services.NotificationServices.Senders.Email.IEmailSender));

                // -- Sms --
                x.Add(NotificationType.Sms, typeof(ISmsSender));

                // -- InApp --
                x.Add(NotificationType.InApp, typeof(IInAppSender));
            });

            return services;
		}
	}
}
