using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Pawfect_API.Data.Entities.EnumTypes;
using Pawfect_API.Data.Entities.HelperModels;

namespace Pawfect_API.Models.Shelter
{
    public class ShelterPersist
    {
        public String Id { get; set; }

        /// <value>
        /// Το id του αντίστοιχου χρήστη που αυτόυ του καταφυγίου
        /// Αναφορά σε User
        /// </value>
        public String UserId { get; set; }

        public String ShelterName { get; set; }

        public String Description { get; set; }


        /// <value>
        /// Η ιστοσελίδα του καταφυγίου
        /// </value>
        public String? Website { get; set; }

        /// <value>
        /// Τα Links για τα Social Media του καταφυγίου
        /// </value>
        public SocialMedia? SocialMedia { get; set; }


        /// <value>
        /// Οι ώρες λειτουργίας του καταφυγίου.
        /// </value>
        public OperatingHours? OperatingHours { get; set; }

        /// <value>
        /// Η κατάσταση αιτήματος εγγραφής του καταφυγίου στο σύστημα.
        /// [ Pending, Verified, Rejected ]
        /// </value>
        /// 
        public VerificationStatus VerificationStatus { get; set; }

        /// <value>
        /// Το id του admin που επιβεβαίωσε την εγγραφή.
        /// Βλέπει σε admin user αφού επιβεβαιωθεί
        /// </value>
        public String? VerifiedBy { get; set; }
    }
}
