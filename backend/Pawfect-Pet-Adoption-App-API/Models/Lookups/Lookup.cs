using System.Reflection;

namespace Pawfect_Pet_Adoption_App_API.Models.Lookups
{
    // Μοντέλο Lookup για τη διαχείριση των επιλογών των GET αιτημάτων
    public abstract class Lookup
    {
        // Φιλτράρισμα
        public int Offset { get; set; }
        public int PageSize { get; set; }
        public string? Query { get; set; }

        private ICollection<string> _fields = new List<string>();
        private ICollection<string> _sortBy = new List<string>();

        public ICollection<string> Fields
        {
            get => _fields;
            set => _fields = value ?? new List<string>();
        }

        // Ταξινόμηση
        public ICollection<string> SortBy
        {
            get => _sortBy;
            set => _sortBy = value ?? new List<string>();
        }
        public bool? SortDescending { get; set; } = false; // Κατεύθυνση ταξινόμησης

        // Μέθοδος για να πάρετε τον τύπο του στοιχείου lookup από τον οποίο καλείται
        // Παράδειγμα AssetLookup -> typeof(Asset)
        public abstract Type GetEntityType();

        // Μέθοδος για τον έλεγχο ενός πεδίου και όλων των ενσωματωμένων πεδίων μέσα σε αυτό με βάση τα δεδομένα του τύπου του
        // Αν δεν συμβεί τίποτα, σημαίνει true, σε περίπτωση εξαίρεσης σημαίνει false
        public virtual void ValidateField(Type entityType, string field)
        {
            // Τα μέρη των ιδιοτήτων για ένα συγκεκριμένο πεδίο
            string[] parts = field.Split('.');

            // Ο τρέχων τύπος που ελέγχουμε αναδρομικά
            Type currentType = entityType;

            foreach (var part in parts)
            {
                PropertyInfo? property = currentType.GetProperty(part);
                // Έλεγχος αν η ιδιότητα δεν είναι ενσωματωμένη
                if (property == null)
                {
                    // * Αρχίστε τον έλεγχο αν το πεδίο είναι έγκυρο εξωτερικό πεδίο, αφού δεν είναι ενσωματωμένο
                    // Εύρεση του σχετικού τύπου στοιχείου (υποθέτοντας ότι η σύμβαση ονομασίας ταιριάζει με το όνομα του τύπου στοιχείου)
                    // Αγνοήστε την περίπτωση για απλότητα του χρήστη
                    Type? relatedEntityType = Assembly.GetExecutingAssembly()
                        .GetTypes()
                        .FirstOrDefault(t => t.Name == part);

                    // Αν το στοιχείο δεν υπάρχει στην εφαρμογή, εκτός βάσης
                    if (relatedEntityType == null)
                    {
                        throw new ArgumentException($"Το εξωτερικό στοιχείο '{part}' δεν αντιστοιχεί σε έγκυρο στοιχείο.");
                    }

                    // Πάρτε όλες τις ιδιότητες των ξένων κλειδιών του στοιχείου
                    List<string> foreignProperties = currentType.GetProperties()
                            .Where(p => p.Name.EndsWith("Id"))
                            .Select(p => p.Name.Substring(0, p.Name.Length - 2))
                            .ToList();

                    // Αν το σχετικό στοιχείο δεν είναι επίσης ξένο κλειδί, εκτός βάσης
                    if (!foreignProperties.Contains(relatedEntityType.Name))
                    {
                        throw new ArgumentException($"Άγνωστος τύπος εξωτερικού στοιχείου: {relatedEntityType.Name} στον τύπο {currentType.Name}");
                    }

                    // Ενημέρωση του τρέχοντος τύπου στοιχείου στον σχετικό τύπο στοιχείου για αναδρομικό έλεγχο
                    currentType = relatedEntityType;

                    continue;
                }
                // Η ιδιότητα επικυρώθηκε, συνεχίζουμε στην επόμενη αναδρομικά
                currentType = property.PropertyType;
            }
        }

        // Μέθοδος για τον έλεγχο όλων των πεδίων για έναν συγκεκριμένο τύπο στοιχείου
        public void ValidateFieldsForEntity(Type entityType)
        {
            foreach (var field in Fields)
            {
                ValidateField(entityType, field);
            }
        }
    }
}
