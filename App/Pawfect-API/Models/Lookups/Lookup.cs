using MongoDB.Bson;
using MongoDB.Driver;
using Pawfect_API.Censors;
using Pawfect_API.Query;
using Pawfect_API.Query.Queries;
using System.Collections;
using System.Reflection;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Pawfect_API.Models.Lookups
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

        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            // Get cached properties (this lookup is very fast after first call per type)
            var properties = _propertyCache.GetOrAdd(GetType(), static type =>
                type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Concat(type.BaseType != typeof(object) && type.BaseType != null ?
                        GetAllProperties(type.BaseType) : Enumerable.Empty<PropertyInfo>())
                    .Where(static p => p.CanRead)
                    .OrderBy(static p => p.Name)
                    .ToArray());

            // Manual hash calculation - fastest approach
            unchecked
            {
                int hash = 17; // Prime seed

                // Unroll the loop for better performance on small property sets
                int len = properties.Length;
                int i = 0;

                // Process properties in groups of 4 for better CPU utilization
                for (; i < len - 3; i += 4)
                {
                    var val0 = properties[i].GetValue(this);
                    var val1 = properties[i + 1].GetValue(this);
                    var val2 = properties[i + 2].GetValue(this);
                    var val3 = properties[i + 3].GetValue(this);

                    hash = hash * 31 + GetValueHashFast(val0);
                    hash = hash * 31 + GetValueHashFast(val1);
                    hash = hash * 31 + GetValueHashFast(val2);
                    hash = hash * 31 + GetValueHashFast(val3);
                }

                // Handle remaining properties
                for (; i < len; i++)
                {
                    var value = properties[i].GetValue(this);
                    hash = hash * 31 + GetValueHashFast(value);
                }

                return hash;
            }
        }

        // Recursive helper to get all properties including from base classes
        private static IEnumerable<PropertyInfo> GetAllProperties(Type type)
        {
            IEnumerable<PropertyInfo> properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                                .Where(p => p.CanRead);

            if (type.BaseType != null && type.BaseType != typeof(object))
            {
                properties = properties.Concat(GetAllProperties(type.BaseType));
            }

            return properties;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetValueHashFast(object? value)
        {
            if (value == null) return 0;

            // Fast path for common types
            switch (value)
            {
                case int i: return i;
                case string s: return s.GetHashCode();
                case bool b: return b ? 1 : 0;
                case DateTime dt: return dt.GetHashCode();
                case ICollection<string> stringColl:
                    return GetStringCollectionHashFast(stringColl);
                case IEnumerable enumerable when value is not string:
                    return GetEnumerableHashFast(enumerable);
                default:
                    return value.GetHashCode();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetStringCollectionHashFast(ICollection<string> collection)
        {
            if (collection == null || collection.Count == 0) return 0;

            unchecked
            {
                int hash = 17;

                if (collection.Count <= 5)
                {
                    foreach (String item in collection)
                    {
                        hash = hash * 31 + (item?.GetHashCode() ?? 0);
                    }
                }
                else
                {
                    foreach (String item in collection.OrderBy(x => x))
                    {
                        hash = hash * 31 + (item?.GetHashCode() ?? 0);
                    }
                }

                return hash;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetEnumerableHashFast(IEnumerable enumerable)
        {
            unchecked
            {
                int hash = 17;
                foreach (var item in enumerable)
                {
                    hash = hash * 31 + (item?.GetHashCode() ?? 0);
                }
                return hash;
            }
        }
    }
}
