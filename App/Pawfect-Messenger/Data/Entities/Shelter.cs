namespace Pawfect_Messenger.Data.Entities
{
	using MongoDB.Bson;
	using MongoDB.Bson.Serialization.Attributes;
    using Pawfect_Messenger.Data.Entities.EnumTypes;
    using Pawfect_Messenger.Data.Entities.HelperModels;

    public class Shelter
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public String Id { get; set; }

		[BsonRepresentation(BsonType.ObjectId)]
		public String UserId { get; set; }

		public String ShelterName { get; set; }

		public String Description { get; set; }

		public String? Website { get; set; }

		public SocialMedia? SocialMedia { get; set; }
		public OperatingHours? OperatingHours { get; set; }
		public VerificationStatus VerificationStatus { get; set; }

		[BsonRepresentation(BsonType.ObjectId)]
		[BsonIgnoreIfNull]
		[BsonIgnoreIfDefault]
		public String? VerifiedById { get; set; }
	}
}