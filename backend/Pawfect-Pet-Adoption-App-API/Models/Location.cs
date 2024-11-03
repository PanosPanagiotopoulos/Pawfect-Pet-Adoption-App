using System.ComponentModel.DataAnnotations;

/// <summary>
///   Η αναλυτική τοποθεσία του καταφυγίου
/// </summary>
public class Location
{
    [Required(ErrorMessage = "Η διέυθυνση είναι απαραίτητη.")]
    [StringLength(100, ErrorMessage = "Λάθος διέυθυνση.")]
    public string Address { get; set; }

    [Required(ErrorMessage = "Ο αριθμός διέυθυνσης είναι απαραίτητος.")]
    [RegularExpression(@"^\d+$", ErrorMessage = "Λάθος αριθμός διέυθυνσης.")]
    [StringLength(10, ErrorMessage = "Λάθος αριθμός διέυθυνσης.")]
    public string Number { get; set; }

    [Required(ErrorMessage = "Η πόλη είναι απαραίτητη.")]
    [StringLength(50, ErrorMessage = "Λάθςο όνομα πόλης")]
    public string City { get; set; }

    [Required(ErrorMessage = "Ο ταχυδρομικός κώδικας είναι απαραίτητος")]
    [RegularExpression(@"^\d{5}(-\d{4})?$", ErrorMessage = "Λάθος ταχυδρομικός κώδικας")]
    public string ZipCode { get; set; }


}