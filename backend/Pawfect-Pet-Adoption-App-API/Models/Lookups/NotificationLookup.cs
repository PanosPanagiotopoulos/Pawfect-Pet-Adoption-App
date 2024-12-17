namespace Pawfect_Pet_Adoption_App_API.Models.Lookups
{
    using Pawfect_Pet_Adoption_App_API.Data.Entities;
    using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
    using Pawfect_Pet_Adoption_App_API.Query.Queries;

    public class NotificationLookup : Lookup
    {
        private NotificationQuery _notificationQuery { get; set; }

        public NotificationLookup(NotificationQuery notificationQuery)
        {
            _notificationQuery = notificationQuery;
        }

        public NotificationLookup() { }

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

        /// <summary>
        /// Εμπλουτίζει το NotificationQuery με τα φίλτρα και τις επιλογές του lookup.
        /// </summary>
        /// <returns>Το εμπλουτισμένο NotificationQuery.</returns>
        public NotificationQuery EnrichLookup(NotificationQuery? toEnrichQuery = null)
        {
            if (toEnrichQuery != null && _notificationQuery == null)
            {
                _notificationQuery = toEnrichQuery;
            }

            // Προσθέτει τα φίλτρα στο NotificationQuery
            _notificationQuery.Ids = this.Ids;
            _notificationQuery.UserIds = this.UserIds;
            _notificationQuery.NotificationTypes = this.NotificationTypes;
            _notificationQuery.CreateFrom = this.CreateFrom;
            _notificationQuery.CreatedTill = this.CreatedTill;
            _notificationQuery.Query = this.Query;

            // Ορίζει επιπλέον επιλογές για το NotificationQuery
            _notificationQuery.PageSize = this.PageSize;
            _notificationQuery.Offset = this.Offset;
            _notificationQuery.SortDescending = this.SortDescending;
            _notificationQuery.Fields = _notificationQuery.FieldNamesOf(this.Fields.ToList());
            _notificationQuery.SortBy = this.SortBy;

            return _notificationQuery;
        }

        /// <summary>
        /// Επιστρέφει τον τύπο οντότητας του NotificationLookup.
        /// </summary>
        /// <returns>Ο τύπος οντότητας του NotificationLookup.</returns>
        public override Type GetEntityType() { return typeof(Notification); }
    }
}
