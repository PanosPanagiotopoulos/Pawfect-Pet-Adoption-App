using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Runtime.CompilerServices;

namespace Main_API.DevTools
{
    /// <summary>
    /// Provides helper methods for JSON serialization.
    /// </summary>
    public static class JsonHelper
    {
        public static String SerializeObjectFormattedSafe(object obj)
        {
            try
            {
                return SerializeObjectFormatted(obj);

            }
            catch (Exception) 
            { 
                return null;
            }
        }

        /// <summary>
        /// Serializes an object to JSON String without including null values.
        /// </summary>
        /// <param name="obj">The object to be serialized.</param>
        /// <returns>The JSON String representation of the object.</returns>
        public static String SerializeObjectFormatted(object obj)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                // Make the JSON output in camel case (general standard)
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                },
                // Format it with \n and \t to be readable
                Formatting = Formatting.Indented
            };

            return JsonConvert.SerializeObject(obj, settings);
        }

        /// <summary>
        /// Safely deserializes a JSON string to an object of type T. Returns default(T) if deserialization fails.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the JSON string to.</typeparam>
        /// <param name="json">The JSON string to be deserialized.</param>
        /// <returns>The deserialized object of type T, or default(T) if deserialization fails.</returns>
        public static T DeserializeObjectFormattedSafe<T>(String json)
        {
            try
            {
                return DeserializeObjectFormatted<T>(json);
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        /// <summary>
        /// Deserializes a JSON string to an object of type T using camel case naming strategy.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the JSON string to.</typeparam>
        /// <param name="json">The JSON string to be deserialized.</param>
        /// <returns>The deserialized object of type T.</returns>
        public static T DeserializeObjectFormatted<T>(String json)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                // Make the JSON input use camel case (general standard)
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                },
                // Handle missing members gracefully
                MissingMemberHandling = MissingMemberHandling.Ignore,
                // Handle null values gracefully
                NullValueHandling = NullValueHandling.Ignore
            };
            return JsonConvert.DeserializeObject<T>(json, settings);
        }
    }
}
