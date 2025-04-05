using System.Reflection;

namespace Pawfect_Pet_Adoption_App_API.Models.Lookups
{
	// Μοντέλο Lookup για τη διαχείριση των επιλογών των GET αιτημάτων
	public abstract class Lookup
	{
		// Φιλτράρισμα
		public int Offset { get; set; }
		public int PageSize { get; set; }
		public String? Query { get; set; }
        public List<String> ExcludeIds { get; set; }

        private ICollection<String> _fields = new List<String>();
		private ICollection<String> _sortBy = new List<String>();

		public ICollection<String> Fields
		{
			get => _fields;
            set => _fields = (value ?? new List<String>())
                     .Select(s =>
                         String.IsNullOrWhiteSpace(s)
                             ? s
                             : char.ToUpper(s[0]) + s.Substring(1))
                     .ToList();
        }

		// Ταξινόμηση
		public ICollection<String> SortBy
		{
			get => _sortBy;
			set => _sortBy = (value ?? new List<String>())
					 .Select(s =>
						 String.IsNullOrWhiteSpace(s)
							 ? s
							 : char.ToUpper(s[0]) + s.Substring(1))
					 .ToList();
		}
		public Boolean? SortDescending { get; set; } = false; // Κατεύθυνση ταξινόμησης

		// Μέθοδος για να πάρετε τον τύπο του στοιχείου lookup από τον οποίο καλείται
		// Παράδειγμα AssetLookup -> typeof(Asset)
		public abstract Type GetEntityType();

		// Μέθοδος για τον έλεγχο ενός πεδίου και όλων των ενσωματωμένων πεδίων μέσα σε αυτό με βάση τα δεδομένα του τύπου του
		// Αν δεν συμβεί τίποτα, σημαίνει true, σε περίπτωση εξαίρεσης σημαίνει false
		public virtual void ValidateField(Type entityType, String field)
		{
			// Τα μέρη των ιδιοτήτων για ένα συγκεκριμένο πεδίο
			String[] parts = field.Split('.');

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
					List<String> foreignProperties = currentType.GetProperties()
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
