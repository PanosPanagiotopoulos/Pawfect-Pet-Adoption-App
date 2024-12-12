using AutoMapper;
using Pawfect_Pet_Adoption_App_API.Models.Notification;

namespace Pawfect_Pet_Adoption_App_API.Builders
{
    public class AutoNotificationBuilder : Profile
    {
        // Builders για μετατροπές object απο Entities σε Μοντέλα κάποιας χρήσης και αντίστροφα
        // Builder για Entity : Notification
        public AutoNotificationBuilder()
        {
            // Mapping για το Entity : Notification σε Notification για χρήση του σε αντιγραφή αντικειμένων
            CreateMap<Notification, Notification>();


            // POST Request Dto Μοντέλα
            CreateMap<Notification, NotificationPersist>();
            CreateMap<NotificationPersist, Notification>();
        }

        // TODO: GET Response Dto Μοντέλα

    }
}