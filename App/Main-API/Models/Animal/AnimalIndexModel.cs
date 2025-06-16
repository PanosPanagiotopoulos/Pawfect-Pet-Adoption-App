namespace Main_API.Models.Animal
{
    // Μοντέλο όπου διατηρεί τα δεδομένα προς index στον Search Server
    public class AnimalIndexModel
    {
        // Id στη Μongo
        public String Id { get; set; }
        // Text representation των στοιχείων προς αναζήτηση για το ζώο
        public String Text { get; set; }
    }
}
