namespace Pawfect_Pet_Adoption_App_API.Models
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;
    using Pawfect_Pet_Adoption_App_API.EnumTypes;
    using System;
    using System.ComponentModel.DataAnnotations;
    /// <summary>
    /// Το μοντέλο ενός ζώου στο σύστημα
    /// </summary>
    public class Animal
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }


        [Required(ErrorMessage = "Το όνομα του ζώου είναι απαραίτητο.")]
        [MinLength(2, ErrorMessage = "Το όνομα του ζώου είναι απαραίτητο.")]
        public string Name { get; set; }


        [Required(ErrorMessage = "Η ηλικία του ζώου είναι απαραίτητη.")]
        [Range(0.1, 40, ErrorMessage = "Λάθος αριθμός ηλικείας σε χρόνια")]
        public double Age { get; set; }

        [Required(ErrorMessage = "Το φύλο του ζώου είναι απαραίτητο.")]
        [BsonRepresentation(BsonType.String)]
        public Gender Gender { get; set; }


        [Required(ErrorMessage = "Η περιγραφή του ζώου είναι απαραίτητη.")]
        [MinLength(10, ErrorMessage = "Η περιγραφή του ζώου πρέπει να έχει τουλάχιστον 10 χαρακτήρες.")]
        public string Description { get; set; }


        [Required(ErrorMessage = "Το βάρος του ζώου είναι απαραίτητο.")]
        [Range(0.1, 150, ErrorMessage = "Λάθος αριθμός βάρους σε κιλά")]
        public double Weight { get; set; }


        /// <value>
        /// Περιγραφή της κατάστασης υγείας του ζώου.
        /// </value>
        [Required(ErrorMessage = "Η κατάσταση υγείας του ζώου είναι απαραίτητη.")]
        [MinLength(8, ErrorMessage = "Παρακαλώ αναγράψτε μια αναλυτικότερη καταφραγή της υγείας του ζωόυ.")]
        public string HealthStatus { get; set; }


        /// <value>
        /// Tο id του καταφυγίου που ανήκει το ζώο.
        /// </value>
        [Required(ErrorMessage = "Το ID του καταφυγίου είναι απαραίτητο.")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ShelterId { get; set; }


        /// <value>
        /// Το id της ράτσας του ζώου στο σύστημα
        /// </value>
        [Required(ErrorMessage = "Το ID της ράτσας είναι απαραίτητο.")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string BreedId { get; set; }


        /// <value>
        /// Το id του τύπου ζώου στο σύστημα    
        /// /// </value>
        [Required(ErrorMessage = "Το ID του τύπου ζώου είναι απαραίτητο.")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string TypeId { get; set; }


        /// <value>
        /// Οι φωτογραφίες του ζώου στο σύστημα , σε μορφη URL όπου είναι αποθηκευμένες στο AWS S3
        /// </value>
        public string[] Photos { get; set; }


        /// <value>
        /// Η κατάσταση υιοθεσίας του ζώου
        /// </value>
        [BsonRepresentation(BsonType.String)]
        public AdoptionStatus AdoptionStatus { get; set; } = AdoptionStatus.Available;


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

}
