using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

using Main_API.Data.Entities.EnumTypes;

namespace Main_API.Data.Entities
{
	/// <summary>
	/// Το μοντέλο μιας αίτησης υιοθεσίας στο σύστημα
	/// </summary>
	public class AdoptionApplication
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public String Id { get; set; }

		[BsonRepresentation(BsonType.ObjectId)]
		public String UserId { get; set; }

		[BsonRepresentation(BsonType.ObjectId)]
		public String AnimalId { get; set; }

		[BsonRepresentation(BsonType.ObjectId)]
		public String ShelterId { get; set; }

		/// <value>
		/// Η κατάσταση του αιτήματος υιοθεσίας
		/// </value>
		public ApplicationStatus Status { get; set; }

		/// <value>
		/// Γενικές πληροφορίες απο τον χρήστη για την υιοθεσία
		/// </value>

		public String ApplicationDetails { get; set; }

		[BsonRepresentation(BsonType.ObjectId)]
		[BsonIgnoreIfNull]
		public List<String>? AttachedFilesIds { get; set; }

		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
	}

}
