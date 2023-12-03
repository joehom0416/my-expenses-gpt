
using System.Text.Json;

namespace MyExpensesGPT.Extensions;

/// <summary>
/// This class provides extension methods for JSON serialization and deserialization.
/// </summary>
public static class JsonExtension
{
    public static JsonSerializerOptions JsonSerializerOption = new JsonSerializerOptions
    {
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Deserializes the specified JSON string into an object of type T.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized object of type T.</returns>
    public static T? DeserializeJson<T>(this string json)
    {
        return JsonSerializer.Deserialize<T>(json, JsonSerializerOption);
    }

    /// <summary>
    /// Serializes the specified object into a JSON string.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>The JSON string representation of the object.</returns>
    public static string? SerializeJson<T>(this T obj)
    {
        return JsonSerializer.Serialize(obj, JsonSerializerOption);
    }
}
