﻿namespace Pawfect_Pet_Adoption_App_API.Data.Entities
{

    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;
    using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
    using Pawfect_Pet_Adoption_App_API.Data.Entities.HelperModels;

    /// <summary>
    /// Το μοντέλο καταφυγίου-χρήστη για το σύστημα με επιπρόσθετα στοιχεία για τον χρήστη αυτου του τύπου
    /// </summary>
    public class Shelter
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        /// <value>
        /// Το id του αντίστοιχου χρήστη που αυτόυ του καταφυγίου
        /// Αναφορά σε User
        /// </value>
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }

        public string ShelterName { get; set; }

        public string Description { get; set; }


        /// <value>
        /// Η ιστοσελίδα του καταφυγίου
        /// </value>
        public string? Website { get; set; }

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
        [BsonRepresentation(BsonType.ObjectId)]
        public string? VerifiedBy { get; set; }
    }
}