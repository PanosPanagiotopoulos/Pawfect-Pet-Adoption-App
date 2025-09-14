namespace Pawfect_Messenger.Data.Entities
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;
    using Pawfect_Messenger.Data.Entities.EnumTypes;
    using System;

    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public String Id { get; set; }
        public String Email { get; set; }
        public String FullName { get; set; }
        public List<UserRole> Roles { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public String ProfilePhotoId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public String ShelterId { get; set; }
        public Boolean IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
