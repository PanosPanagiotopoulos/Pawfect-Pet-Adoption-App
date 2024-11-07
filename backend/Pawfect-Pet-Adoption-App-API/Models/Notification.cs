using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Pawfect_Pet_Adoption_App_API.Models.EnumTypes;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Το μοντέλο μιας ειδοποίησης στο σύστημα
/// </summary>
public class Notification
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [Required(ErrorMessage = "Το ID του χρήστη είναι απαραίτητο.")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; }

    [Required(ErrorMessage = "Ο τύπος της ειδοποίησης είναι απαραίτητος.")]
    [BsonRepresentation(BsonType.String)]
    public NotificationType Type { get; set; }

    [Required(ErrorMessage = "Το περιεχόμενο της ειδοποίησης είναι απαραίτητο.")]
    public string Content { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}