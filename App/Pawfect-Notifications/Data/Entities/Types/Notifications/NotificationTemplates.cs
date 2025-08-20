namespace Pawfect_Notifications.Data.Entities.Types.Notifications
{
    public class NotificationTemplates
    {
        public List<NotificationTemplate> Templates { get; set; }
    }

    public class NotificationTemplate
    {
        public Guid TemplateId { get; set; }
        public String TitlePath { get; set; }
        public String ContentPath { get; set; }
    }

}
