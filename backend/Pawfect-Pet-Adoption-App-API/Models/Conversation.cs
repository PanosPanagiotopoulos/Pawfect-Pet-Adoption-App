using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Το μοντέλο μιας συνομιλίας στο σύστημα
/// </summary>
public class Conversation
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [Required(ErrorMessage = "Τα id των συμμετεχόντών είναι απαραίτητα.")]
    [BsonElement("users")]
    /// <value>
    /// Τα id των χρηστών της συζήτησης
    /// </value>
    [BsonRepresentation(BsonType.ObjectId)]
    public List<string> Users { get; set; } = new List<string>();

    /// <value>
    /// Το id του ζώου όπου αναφέρεται η συνομηλία
    /// </value>
    [Required(ErrorMessage = "Το ζώο για το οποίο αναφέρεται η συζήτηση είναι απαραίτητο")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string AnimalId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}