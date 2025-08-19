namespace Main_API.Data.Entities
{
	using MongoDB.Bson;
	using MongoDB.Bson.Serialization.Attributes;

	using Main_API.Data.Entities.EnumTypes;

	using System;
    using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Embedding;

    /// <summary>
    /// Το μοντέλο ενός ζώου στο σύστημα
    /// </summary>
    public class Animal
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public String Id { get; set; }


		public String Name { get; set; }


		public Double Age { get; set; }

        [BsonRepresentation(BsonType.String)]
        public Gender Gender { get; set; }


		public String Description { get; set; }


		public Double Weight { get; set; }


		/// <value>
		/// Περιγραφή της κατάστασης υγείας του ζώου.
		/// </value>
		public String HealthStatus { get; set; }


		/// <value>
		/// Tο id του καταφυγίου που ανήκει το ζώο.
		/// </value>
		[BsonRepresentation(BsonType.ObjectId)]
		public String ShelterId { get; set; }


		/// <value>
		/// Το id της ράτσας του ζώου στο σύστημα
		/// </value>
		[BsonRepresentation(BsonType.ObjectId)]
		public String BreedId { get; set; }


		/// <value>
		/// Το id του τύπου ζώου στο σύστημα    
		/// /// </value>
		[BsonRepresentation(BsonType.ObjectId)]
		public String AnimalTypeId { get; set; }


		/// <value>
		/// Οι φωτογραφίες του ζώου στο σύστημα , σε μορφη FileId όπου είναι αποθηκευμένες στο AWS S3
		/// </value>
		[BsonRepresentation(BsonType.ObjectId)]
		[BsonIgnoreIfNull]
		public List<String> PhotosIds { get; set; }


		/// <value>
		/// Η κατάσταση υιοθεσίας του ζώου
		/// </value>
		public AdoptionStatus AdoptionStatus { get; set; }

        [BsonIgnoreIfNull]
        public Double[] Embedding { get; set; }

        [BsonIgnoreIfNull]
        public String SemanticText { get; set; }

        [BsonIgnoreIfNull]
        public Double? SearchScore { get; set; }

        public DateTime CreatedAt { get; set; }

		public DateTime UpdatedAt { get; set; }
	}

}
