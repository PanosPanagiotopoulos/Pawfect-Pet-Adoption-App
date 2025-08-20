namespace Pawfect_Notifications.Data.Entities
{
	using MongoDB.Bson;
	using MongoDB.Bson.Serialization.Attributes;

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
        public String Phone { get; set; }
        public String FullName { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
	}

}
