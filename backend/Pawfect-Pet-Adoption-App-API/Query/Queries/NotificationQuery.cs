﻿using MongoDB.Driver;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Models.Notification;
using Pawfect_Pet_Adoption_App_API.Services;

namespace Pawfect_Pet_Adoption_App_API.Query.Queries
{
    public class NotificationQuery : BaseQuery<Notification>
    {
        // Κατασκευαστής για την κλάση NotificationQuery
        // Είσοδος: mongoDbService - μια έκδοση της κλάσης MongoDbService
        public NotificationQuery(MongoDbService mongoDbService)
        {
            base._collection = mongoDbService.GetCollection<Notification>();
        }

        // Λίστα με τα IDs των ειδοποιήσεων για φιλτράρισμα
        public List<string>? Ids { get; set; }

        // Λίστα με τα IDs των χρηστών για φιλτράρισμα
        public List<string>? UserIds { get; set; }

        // Λίστα με τους τύπους ειδοποιήσεων για φιλτράρισμα
        public List<NotificationType>? NotificationTypes { get; set; }

        // Ημερομηνία έναρξης για φιλτράρισμα
        public DateTime? CreateFrom { get; set; }

        // Ημερομηνία λήξης για φιλτράρισμα
        public DateTime? CreatedTill { get; set; }

        // Εφαρμόζει τα καθορισμένα φίλτρα στο ερώτημα
        // Έξοδος: FilterDefinition<Notification> - ο ορισμός φίλτρου που θα χρησιμοποιηθεί στο ερώτημα
        protected override Task<FilterDefinition<Notification>> ApplyFilters()
        {
            FilterDefinitionBuilder<Notification> builder = Builders<Notification>.Filter;
            FilterDefinition<Notification> filter = builder.Empty;

            // Εφαρμόζει φίλτρο για τα IDs των ειδοποιήσεων
            if (Ids != null && Ids.Any())
            {
                filter &= builder.In(notification => notification.Id, Ids);
            }

            // Εφαρμόζει φίλτρο για τα IDs των χρηστών
            if (UserIds != null && UserIds.Any())
            {
                filter &= builder.In(notification => notification.UserId, UserIds);
            }

            // Εφαρμόζει φίλτρο για τους τύπους ειδοποιήσεων
            if (NotificationTypes != null && NotificationTypes.Any())
            {
                filter &= builder.In(notification => notification.Type, NotificationTypes);
            }

            // Εφαρμόζει φίλτρο για την ημερομηνία έναρξης
            if (CreateFrom.HasValue)
            {
                filter &= builder.Gte(notification => notification.CreatedAt, CreateFrom.Value);
            }

            // Εφαρμόζει φίλτρο για την ημερομηνία λήξης
            if (CreatedTill.HasValue)
            {
                filter &= builder.Lte(notification => notification.CreatedAt, CreatedTill.Value);
            }

            return Task.FromResult(filter);
        }
        // Επιστρέφει τα ονόματα πεδίων που θα προβληθούν στο αποτέλεσμα του ερωτήματος
        // Είσοδος: fields - μια λίστα με τα ονόματα των πεδίων που θα προβληθούν
        // Έξοδος: List<string> - τα ονόματα των πεδίων που θα προβληθούν
        public override List<string> FieldNamesOf(List<string> fields)
        {
            if (fields == null) return new List<string>();
            if (fields.Any() || fields.Contains("*")) return EntityHelper.GetAllPropertyNames(typeof(Notification)).ToList();

            HashSet<string> projectionFields = new HashSet<string>();
            foreach (string item in fields)
            {
                // Αντιστοιχίζει τα ονόματα πεδίων NotificationDto στα ονόματα πεδίων Notification
                if (item.Equals(nameof(NotificationDto.Id))) projectionFields.Add(nameof(Notification.Id));
                if (item.Equals(nameof(NotificationDto.Type))) projectionFields.Add(nameof(Notification.Type));
                if (item.Equals(nameof(NotificationDto.Content))) projectionFields.Add(nameof(Notification.Content));
                if (item.Equals(nameof(NotificationDto.IsRead))) projectionFields.Add(nameof(Notification.IsRead));
                if (item.Equals(nameof(NotificationDto.CreatedAt))) projectionFields.Add(nameof(Notification.CreatedAt));
                if (item.StartsWith(nameof(NotificationDto.User))) projectionFields.Add(nameof(Notification.UserId));
            }
            return projectionFields.ToList();
        }
    }
}