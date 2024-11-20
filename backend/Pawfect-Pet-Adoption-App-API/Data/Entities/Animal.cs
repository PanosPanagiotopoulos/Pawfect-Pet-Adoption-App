namespace Pawfect_Pet_Adoption_App_API.Data.Entities
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;
    using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
    using System;
    /// <summary>
    /// Το μοντέλο ενός ζώου στο σύστημα
    /// </summary>
    public class Animal
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }


        public string Name { get; set; }


        public double Age { get; set; }

        public Gender Gender { get; set; }


        public string Description { get; set; }


        public double Weight { get; set; }


        /// <value>
        /// Περιγραφή της κατάστασης υγείας του ζώου.
        /// </value>
        public string HealthStatus { get; set; }


        /// <value>
        /// Tο id του καταφυγίου που ανήκει το ζώο.
        /// </value>
        public string ShelterId { get; set; }


        /// <value>
        /// Το id της ράτσας του ζώου στο σύστημα
        /// </value>
        public string BreedId { get; set; }


        /// <value>
        /// Το id του τύπου ζώου στο σύστημα    
        /// /// </value>
        public string TypeId { get; set; }


        /// <value>
        /// Οι φωτογραφίες του ζώου στο σύστημα , σε μορφη URL όπου είναι αποθηκευμένες στο AWS S3
        /// </value>
        public string[] Photos { get; set; }


        /// <value>
        /// Η κατάσταση υιοθεσίας του ζώου
        /// </value>
        public AdoptionStatus AdoptionStatus { get; set; }


        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }

}
