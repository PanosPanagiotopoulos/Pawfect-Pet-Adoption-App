using MongoDB.Bson;
using MongoDB.Driver;
using Main_API.Censors;
using Main_API.Query;
using Main_API.Query.Queries;
using System.Collections;
using System.Reflection;

namespace Main_API.Models.Lookups
{
	public abstract class Lookup
	{
        // Φιλτράρισμα
        public int Offset { get; set; } = 1;
        public int PageSize { get; set; } = 10000;
		public String? Query { get; set; }

        private ICollection<String> _fields = new List<String>();
		private ICollection<String> _sortBy = new List<String>();

		public ICollection<String> Fields
		{
			get => _fields;
            set => _fields = BaseCensor.PrepareFieldsList([..value]);
        }

		// Ταξινόμηση
		public ICollection<String> SortBy
		{
			get => _sortBy;
			set => _sortBy = BaseCensor.PrepareFieldsList([.. value]);
        }

        public Boolean? SortDescending { get; set; } = false; // Κατεύθυνση ταξινόμησης


        // Μέθοδος για να πάρετε τον τύπο του στοιχείου lookup από τον οποίο καλείται
        // Παράδειγμα AssetLookup -> typeof(Asset)
        public abstract Type GetEntityType();
        public abstract Task<FilterDefinition<BsonDocument>> ToFilters(IQueryFactory queryFactory);

        public void EnrichCommon(IQuery query)
        {
            query.PageSize = this.PageSize;
            query.Offset = this.Offset;
            query.SortDescending = this.SortDescending;
            query.SortBy = this.SortBy;
        }
        // Ορίζει επιπλέον επιλογές για το AdoptionApplicationQuery
        

        // Μέθοδος για τον έλεγχο όλων των πεδίων για έναν συγκεκριμένο τύπο στοιχείου
        public void ValidateFieldsForEntity(Type entityType)
        {
            foreach (String field in Fields)
            {
                this.ValidateField(entityType, field);
            }
        }
        // Μέθοδος για τον έλεγχο ενός πεδίου και όλων των ενσωματωμένων πεδίων μέσα σε αυτό με βάση τα δεδομένα του τύπου του
        // Αν δεν συμβεί τίποτα, σημαίνει true, σε περίπτωση εξαίρεσης σημαίνει false
        public virtual void ValidateField(Type entityType, String field)
		{
			// Τα μέρη των ιδιοτήτων για ένα συγκεκριμένο πεδίο
			String[] parts = field.Split('.');

			// Ο τρέχων τύπος που ελέγχουμε αναδρομικά
			Type currentType = entityType;

			foreach (String part in parts)
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

        // Hash methods

        protected virtual int GetDerivedHashCode()
        {
            int hash = 23;
            var derivedType = this.GetType();
            var derivedProperties = derivedType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.DeclaringType == derivedType)
                .OrderBy(p => p.Name);

            foreach (var property in derivedProperties)
            {
                object value = property.GetValue(this);
                int propertyHash = this.GetPropertyHashCode(value);
                hash = this.CombineHash(hash, propertyHash);
            }

            return hash;
        }

        public override int GetHashCode()
        {
            int hash = 17; // Initial prime number

            // Get all public instance properties of the base class
            IOrderedEnumerable<PropertyInfo> baseProperties = typeof(Lookup).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .OrderBy(p => p.Name);

            // Process each base property
            foreach (PropertyInfo property in baseProperties)
            {
                object value = property.GetValue(this);
                int propertyHash = GetPropertyHashCode(value);
                hash = CombineHash(hash, propertyHash);
            }

            // Combine with the derived class's hash code
            int derivedHash = this.GetDerivedHashCode();
            hash = this.CombineHash(hash, derivedHash);

            return hash;
        }

        protected int GetPropertyHashCode(object value)
        {
            if (value == null)
                return 0;

            if (value is IEnumerable enumerable && !(value is String))
                return this.GetCollectionHashCode(enumerable);

            return value.GetHashCode();
        }

        // Compute hash code for collections, ensuring order independence
        protected int GetCollectionHashCode(IEnumerable collection)
        {
            int hash = 19; // Different initial prime for collections

            var sortedItems = collection.Cast<object>()
                .OrderBy(item => item?.GetHashCode() ?? 0);

            foreach (object item in sortedItems)
            {
                int itemHash = item?.GetHashCode() ?? 0;
                hash = CombineHash(hash, itemHash);
            }

            return hash;
        }

        // Combine two hash codes using bit manipulation to avoid overflow
        protected int CombineHash(int h1, int h2)
        {
            // Rotate left by 5 bits and XOR with h2
            int rol5 = (h1 << 5) | (h1 >> 27);
            return rol5 ^ h2;
        }
    }
}
