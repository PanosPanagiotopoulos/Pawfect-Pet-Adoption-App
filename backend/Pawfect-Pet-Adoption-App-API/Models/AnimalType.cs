﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Pawfect_Pet_Adoption_App_API.Models
{
    /// <summary>
    /// Το μοντέλο ενός τύπου ζώου στο σύστημα
    /// </summary>
    public class AnimalType
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
