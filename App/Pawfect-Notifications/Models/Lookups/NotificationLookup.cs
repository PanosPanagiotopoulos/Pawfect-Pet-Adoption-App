namespace Pawfect_Notifications.Models.Lookups
{
    using MongoDB.Bson;
    using MongoDB.Driver;
    using Pawfect_Notifications.Data.Entities;
    using Pawfect_Notifications.Data.Entities.EnumTypes;
    using Pawfect_Notifications.DevTools;
    using Pawfect_Notifications.Query;
    using Pawfect_Notifications.Query.Queries;

    public class NotificationLookup : Lookup
    {
        // Λίστα με τα IDs των ειδοποιήσεων για φιλτράρισμα
        public List<String>? Ids { get; set; }

        public List<String>? ExcludedIds { get; set; }


        // Λίστα με τα IDs των χρηστών για φιλτράρισμα
        public List<String>? UserIds { get; set; }

        // Λίστα με τους τύπους ειδοποιήσεων για φιλτράρισμα
        public List<NotificationType>? NotificationTypes { get; set; }

        public Boolean? IsRead { get; set; }

        // Ημερομηνία έναρξης για φιλτράρισμα
        public DateTime? CreateFrom { get; set; }

        // Ημερομηνία λήξης για φιλτράρισμα
        public DateTime? CreatedTill { get; set; }

        /// <summary>
        /// Εμπλουτίζει το NotificationQuery με τα φίλτρα και τις επιλογές του lookup.
        /// </summary>
        /// <returns>Το εμπλουτισμένο NotificationQuery.</returns>
        public NotificationQuery EnrichLookup(IQueryFactory queryFactory)
        {
            NotificationQuery notificationQuery = queryFactory.Query<NotificationQuery>();

            // Προσθέτει τα φίλτρα στο NotificationQuery με if statements
            if (this.Ids != null && this.Ids.Count != 0) notificationQuery.Ids = this.Ids;
            if (this.ExcludedIds != null && this.ExcludedIds.Count != 0) notificationQuery.ExcludedIds = this.ExcludedIds;
            if (this.UserIds != null && this.UserIds.Count != 0) notificationQuery.UserIds = this.UserIds;
            if (this.NotificationTypes != null && this.NotificationTypes.Count != 0) notificationQuery.NotificationTypes = this.NotificationTypes;
            if (this.CreateFrom.HasValue) notificationQuery.CreateFrom = this.CreateFrom;
            if (this.CreatedTill.HasValue) notificationQuery.CreatedTill = this.CreatedTill;
            if (this.IsRead.HasValue) notificationQuery.IsRead = this.IsRead;
            if (!String.IsNullOrEmpty(this.Query)) notificationQuery.Query = this.Query;

            notificationQuery.Fields = notificationQuery.FieldNamesOf([.. this.Fields]);

            base.EnrichCommon(notificationQuery);

            return notificationQuery;
        }

        public override async Task<FilterDefinition<BsonDocument>> ToFilters(IQueryFactory queryFactory)
        {
            FilterDefinition<Data.Entities.Notification> filters = await this.EnrichLookup(queryFactory).ApplyFilters();

            return MongoHelper.ToBsonFilter(filters);
        }

        /// <summary>
        /// Επιστρέφει τον τύπο οντότητας του NotificationLookup.
        /// </summary>
        /// <returns>Ο τύπος οντότητας του NotificationLookup.</returns>
        public override Type GetEntityType() { return typeof(Notification); }
    }
}
