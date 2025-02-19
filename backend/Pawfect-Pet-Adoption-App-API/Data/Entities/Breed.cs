﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Pawfect_Pet_Adoption_App_API.Data.Entities
{
    /// <summary>
    /// Το μοντέλο μιας ράτσας ζώου στο σύστημα
    /// </summary>
    public class Breed
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public String Id { get; set; }

        public String Name { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public String TypeId { get; set; }
        public String Description { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
