namespace Pawfect_Pet_Adoption_App_API.Data.Entities
{
	using MongoDB.Bson;
	using MongoDB.Bson.Serialization.Attributes;

	using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;

	using System;

	/// <summary>
	/// Το κύριο μοντέλο ενώς χρήστη στο σύστημα
	/// </summary>
	public class User
	{

		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public String Id { get; set; }

		public String Email { get; set; }

		[BsonIgnoreIfNull]
		public String? Password { get; set; } // Only needed for local credentials


		public String FullName { get; set; }


		/// <value>
		/// Ο ρόλος του χρήστη στο σύστημα.
		/// [ User, Shelter, Admin ]
		/// </value>
		public List<UserRole> Roles { get; set; }

		/// <value>
		/// Ο αριθμός τηλεφώνου του χρήστη στο σύστημα
		/// </value>
		public String Phone { get; set; }


		/// <value>
		/// Η τοποθεσία του χρήστη
		/// </value>
		public Location Location { get; set; }

		/// <value>
		/// Το id καταφυγίου με τα υπόλοιπα δεδομένα για τον συγκεκριμένο χρήστη.
		/// </value>
		/// Μόνο για χρήστες-καταφύγια
		[BsonRepresentation(BsonType.ObjectId)]
		public String? ShelterId { get; set; }


		/// <value>
		/// Ο τρόπος πρόσβασης του χρήστη. Π.χ Local άν συνδέεται με email, password ή Google άν συνδέεται με google.
		/// </value>
		/// [ Google, Local ]
		[BsonRepresentation(BsonType.String)]
		public AuthProvider AuthProvider { get; set; }

		/// <value>
		/// To id του χρήστη στην εξωτερική υπηρεσία που επέλεξε να εγγραφεί/συνδεθεί
		/// </value>
		[BsonIgnoreIfNull]
		public String? AuthProviderId { get; set; }

		[BsonRepresentation(BsonType.ObjectId)]
		[BsonIgnoreIfNull]
		public String ProfilePhotoId { get; set; }

		/// <value>
		/// Υποδηλώνει άν τα στοιχεία του χρήστη έχουν επιβεβαιωθεί
		/// </value>
		public Boolean IsVerified { get; set; }

		/// <value>
		/// Υποδηλώνει άν το κινητό του χρήστη έχουν επιβεβαιωθεί
		/// </value>
		public Boolean HasPhoneVerified { get; set; }
		/// <value>
		/// Υποδηλώνει άν το email του χρήστη έχουν επιβεβαιωθεί
		/// </value>
		public Boolean HasEmailVerified { get; set; }

		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
	}

}
