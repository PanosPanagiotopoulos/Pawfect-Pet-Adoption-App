using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Pawfect_Pet_Adoption_App_API.EnumTypes;
using Pawfect_Pet_Adoption_App_API.Models;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Το μοντέλο καταφυγίου-χρήστη για το σύστημα με επιπρόσθετα στοιχεία για τον χρήστη αυτου του τύπου
/// </summary>
public class Shelter
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    /// <value>
    /// Το id του αντίστοιχου χρήστη που αυτόυ του καταφυγίου
    /// </value>
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } // Reference to the User model

    [Required]
    [MinLength(2, ErrorMessage = "Το όνομα καταφυγίου πρέπει να έχει τουλάχιστον 2 χαρακτήρες.")]
    public string ShelterName { get; set; }

    [Required]
    [MinLength(10, ErrorMessage = "Η περιγραφή πρέπει να έχει τουλάχιστον 10 χαρακτήρες.")]
    public string Description { get; set; }


    /// <value>
    /// Η ιστοσελίδα του καταφυγίου
    /// </value>
    [Url(ErrorMessage = "Λάθος καταφραφή Link ιστοσελίδας.")]
    public string? Website { get; set; }

    /// <value>
    /// Τα Links για τα Social Media του καταφυγίου
    /// </value>
    public SocialMedia? SocialMedia { get; set; }


    /// <value>
    /// Οι ώρες λειτουργίας του καταφυγίου.
    /// </value>
    public OperatingHours? OperatingHours { get; set; }


    /// <value>
    /// Η κατάσταση αιτήματος εγγραφής του καταφυγίου στο σύστημα.
    /// </value>
    [BsonRepresentation(BsonType.String)]
    public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending; // Enum: Pending, Verified, Rejected


    /// <value>
    /// Το id του admin που επιβεβαίωσε την εγγραφή.
    /// </value>
    [BsonRepresentation(BsonType.ObjectId)]
    public string? VerifiedBy { get; set; } // Reference to Admin user
}
