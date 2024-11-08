namespace Pawfect_Pet_Adoption_App_API.DevTools
{
    using System.Text.Json;
    using System.Text.Json.Serialization;

    namespace Pawfect_Pet_Adoption_App_API.DevTools
    {
        public class JsonStringToEnumConverter<T> : JsonConverter<T> where T : struct, Enum
        {
            public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                string? enumText = reader.GetString();

                if (Enum.TryParse(enumText, ignoreCase: true, out T value) && Enum.IsDefined(typeof(T), value))
                {
                    return value;
                }

                throw new JsonException($"Unable to convert '{enumText}' to  '{typeof(T).Name}' .");
            }

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString());
            }
        }
    }
}
