using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;

namespace Pawfect_Pet_Adoption_App_API.Builders
{
    public abstract class BaseBuilder<M, E>
    {
        protected Lookup? LookupParams { get; set; }

        public abstract BaseBuilder<M, E> SetLookup(Lookup lookup);

        public abstract Task<List<M>> BuildDto(List<E> entities, List<string> fields);
        protected virtual (List<string>, Dictionary<string, List<string>>) ExtractBuildFields(List<string> fields)
        {
            // 1) Πάρτε τα πεδία του entity που πρέπει να είναι στη λίστα fields
            List<string> entityProperties = EntityHelper.GetAllPropertyNames(typeof(E)).ToList();

            // Η λίστα επιστροφής των ξένων κλειδιών του entity
            Dictionary<string, List<string>> foreignEntityFields = new Dictionary<string, List<string>>();
            List<string> nativeFields = new List<string>();

            // Αν τα πεδία είναι άδεια ή null, πάρτε όλα τα native πεδία μόνο
            if (fields == null || !fields.Any())
            {
                return (entityProperties, foreignEntityFields);
            }

            foreach (string field in fields)
            {
                /*
                 * Αν η ιδιότητα δεν είναι στα πεδία, αυτό σημαίνει ότι είναι ιδιότητα άλλου entity
                 * Έτσι το χειριζόμαστε γνωρίζοντας ότι θα είναι ιδιότητες με τη σύνταξη: "entity.property"
                */
                if (!entityProperties.Contains(field))
                {
                    // Πάρτε το πεδίο που είναι ξένο και θα χρειαστεί να το προσπελάσετε μέσω ερωτήματος ξένου κλειδιού
                    // Για παράδειγμα, μια είσοδος μπορεί να είναι: WorkPosition.Name.Branch. Χρειαζόμαστε το Name.Branch για να το περάσουμε σε έναν άλλο builder
                    string[] propertyFields = field.Split(".");

                    string foreignEntity = propertyFields[0];
                    string foreignFieldProperty = string.Join(".", propertyFields.Skip(1));

                    // Προσθέστε το ξένο πεδίο στα build fields
                    if (!foreignEntityFields.ContainsKey(foreignEntity))
                    {
                        foreignEntityFields[foreignEntity] = new List<string>();
                    }
                    foreignEntityFields[foreignEntity].Add(foreignFieldProperty);

                    continue;
                }

                // Αν το πεδίο είναι μέσα στη λίστα ιδιοτήτων, αυτό σημαίνει ότι είναι το καθαρό όνομα μιας ιδιότητας του entity
                nativeFields.Add(field);
            }

            // Επιστροφή και των δύο λιστών ως tuple
            return (nativeFields, foreignEntityFields);
        }
    }
}
