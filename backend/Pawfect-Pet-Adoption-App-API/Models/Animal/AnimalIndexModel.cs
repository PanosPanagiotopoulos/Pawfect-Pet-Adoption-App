namespace Pawfect_Pet_Adoption_App_API.Models.Animal
{
    // Μοντέλο όπου διατηρεί τα δεδομένα προς index στον Search Server
    public class AnimalIndexModel
    {
        // Id στη Μongo
        public string Id { get; set; }
        // Text representation των στοιχείων προς αναζήτηση για το ζώο
        public string Text { get; set; }
    }
}
