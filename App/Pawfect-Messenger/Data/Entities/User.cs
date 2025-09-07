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
        public String ProfilePhotoId { get; set; }
        public String ShelterId { get; set; }
        public Boolean IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // SignalR
        public List<String> ConnectionIds { get; set; } = new List<String>();
        public UserStatus Status { get; set; } = UserStatus.Offline;
        [BsonIgnoreIfNull]
        public DateTime? LastSeen { get; set; }
       
    }

}
