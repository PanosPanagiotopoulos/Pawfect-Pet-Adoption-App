using AutoMapper;
using Pawfect_Pet_Adoption_App_API.Models.Notification;

namespace Pawfect_Pet_Adoption_App_API.Builders
{
    public class NotificationBuilder : Profile
    {
        // Builders για μετατροπές object απο Entities σε Μοντέλα κάποιας χρήσης και αντίστροφα
        // Builder για Entity : Notification
        public NotificationBuilder()
        {
            // Mapping για το Entity : Notification σε Notification για χρήση του σε αντιγραφή αντικειμένων
            CreateMap<Notification, Notification>();

            // GET Response Dto Μοντέλα
            CreateMap<Notification, NotificationDto>();
            CreateMap<NotificationDto, Notification>();

            // POST Request Dto Μοντέλα
            CreateMap<Notification, NotificationPersist>();
            CreateMap<NotificationPersist, Notification>();
        }
    }
}