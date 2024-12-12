using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Pawfect_Pet_Adoption_App_API.DevTools
{
    /// <summary>
    /// Provides helper methods for JSON serialization.
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// Serializes an object to JSON string without including null values.
        /// </summary>
        /// <param name="obj">The object to be serialized.</param>
        /// <returns>The JSON string representation of the object.</returns>
        public static string SerializeObjectFormatted(object obj)
        {
            var settings = new JsonSerializerSettings
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
    }
}
