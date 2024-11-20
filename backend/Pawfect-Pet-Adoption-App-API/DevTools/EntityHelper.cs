using MongoDB.Bson;

namespace Pawfect_Pet_Adoption_App_API.DevTools
{
    public static class EntityHelper
    {
        /// <summary>
        /// Gets all the column names of the specified type.
        /// </summary>
        /// <param name="type">The type to get the column names from.</param>
        /// <returns>A collection of column names.</returns>
        public static ICollection<string> GetAllPropertyNames(Type type)
        {
            return type.GetProperties().Select(p => p.Name).ToList();
        }

        /// <summary>
        /// Checks if the specified value is the default value for its type.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>True if the value is the default value, otherwise false.</returns>
        public static bool IsDefaultValue(object value)
        {
            if (value == null) return true;

            var type = value.GetType();

            // If object is of string type
            if (type == typeof(string))
            {
                return string.IsNullOrEmpty((string)value);
            }

            // If it is any other object    
            if (type.IsValueType)
            {
                return value.Equals(Activator.CreateInstance(type));
            }

            // If it is a MongoDB ObjectId
            if (type == typeof(ObjectId))
            {
                return ObjectId.Empty.Equals(value);
            }


            return false;
        }
    }
}
