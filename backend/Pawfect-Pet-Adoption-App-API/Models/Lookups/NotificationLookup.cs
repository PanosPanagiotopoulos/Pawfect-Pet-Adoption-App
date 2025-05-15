namespace Pawfect_Pet_Adoption_App_API.Models.Lookups
{
    using Pawfect_Pet_Adoption_App_API.Data.Entities;
    using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
    using Pawfect_Pet_Adoption_App_API.Query;
    using Pawfect_Pet_Adoption_App_API.Query.Queries;

    public class NotificationLookup : Lookup
    {
        // Λίστα με τα IDs των ειδοποιήσεων για φιλτράρισμα
        public List<String>? Ids { get; set; }

        public List<String>? ExcludedIds { get; set; }


        // Λίστα με τα IDs των χρηστών για φιλτράρισμα
        public List<String>? UserIds { get; set; }

        // Λίστα με τους τύπους ειδοποιήσεων για φιλτράρισμα
        public List<NotificationType>? NotificationTypes { get; set; }

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

            // Προσθέτει τα φίλτρα στο NotificationQuery
            notificationQuery.Ids = this.Ids;
            notificationQuery.UserIds = this.UserIds;
            notificationQuery.NotificationTypes = this.NotificationTypes;
            notificationQuery.CreateFrom = this.CreateFrom;
            notificationQuery.CreatedTill = this.CreatedTill;
            notificationQuery.Query = this.Query;

            // Ορίζει επιπλέον επιλογές για το NotificationQuery
            notificationQuery.PageSize = this.PageSize;
            notificationQuery.Offset = this.Offset;
            notificationQuery.SortDescending = this.SortDescending;
            notificationQuery.Fields = notificationQuery.FieldNamesOf(this.Fields.ToList());
            notificationQuery.SortBy = this.SortBy;
            notificationQuery.ExcludedIds = this.ExcludedIds;

            return notificationQuery;
        }

        /// <summary>
        /// Επιστρέφει τον τύπο οντότητας του NotificationLookup.
        /// </summary>
        /// <returns>Ο τύπος οντότητας του NotificationLookup.</returns>
        public override Type GetEntityType() { return typeof(Notification); }
    }
}
