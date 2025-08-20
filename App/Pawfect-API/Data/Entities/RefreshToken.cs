using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Pawfect_API.Data.Entities
{
    public class RefreshToken
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public String Id { get; set; }
        public String Token { get; set; }
        public String LinkedTo { get; set; }
        public String Ip { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
