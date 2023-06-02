using System.Buffers;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text;

namespace BillyCDK.App.Utilities;

public class JsonUtils
{
    // Adapted from: https://github.com/dotnet/runtime/issues/31433#issuecomment-570475853
    public static JsonElement Merge(JsonElement originalJson, JsonElement newContent)
    {
        var outputBuffer = new ArrayBufferWriter<byte>();

        using (var jsonWriter = new Utf8JsonWriter(outputBuffer, new JsonWriterOptions { Indented = true }))
        {

            if (originalJson.ValueKind != JsonValueKind.Array && originalJson.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidOperationException($"The original JSON document to merge new content into must be a container type. Instead it is {originalJson.ValueKind}.");
            }

            if (originalJson.ValueKind != newContent.ValueKind)
            {
                throw new InvalidOperationException($"The original JSON document is of type {originalJson.ValueKind}, which cannot be merged with type {newContent.ValueKind}");
            }

            if (originalJson.ValueKind == JsonValueKind.Array)
            {
                MergeArrays(jsonWriter, originalJson, newContent);
            }
            else
            {
                MergeObjects(jsonWriter, originalJson, newContent);
            }
        }

        string jsonLiteral = Encoding.UTF8.GetString(outputBuffer.WrittenSpan);

        return JsonDocument.Parse(jsonLiteral).RootElement;
    }

    public static JsonDocument CreateCompactDocument(string originalJson)
    {
        // remove all white spaces
        var pattern = @"\s+(?=(?:[^\'""]*[\'""][^\'""]*[\'""])*[^\'""]*$)";
        string compactJsonLiteral = Regex.Replace(originalJson, pattern, "");

        // validate and return json element
        return JsonDocument.Parse(compactJsonLiteral);
    }

    public static string CreateCompactLiteral(string originalJson)
        => CreateCompactDocument(originalJson).RootElement.ToString();

    public static string CreateLiteral(string originalJson)
        => JsonDocument.Parse(originalJson).RootElement.ToString();

    #region Private methods
    private static void MergeObjects(Utf8JsonWriter jsonWriter, JsonElement root1, JsonElement root2)
    {

        jsonWriter.WriteStartObject();

        // Write all the properties of the first document.
        // If a property exists in both documents, either:
        // * Merge them, if the value kinds match (e.g. both are objects or arrays),
        // * Completely override the value of the first with the one from the second, if the value kind mismatches (e.g. one is object, while the other is an array or string),
        // * Or favor the value of the first (regardless of what it may be), if the second one is null (i.e. don't override the first).
        foreach (JsonProperty property in root1.EnumerateObject())
        {
            string propertyName = property.Name;

            JsonValueKind newValueKind;

            if (root2.TryGetProperty(propertyName, out JsonElement newValue) && (newValueKind = newValue.ValueKind) != JsonValueKind.Null)
            {
                jsonWriter.WritePropertyName(propertyName);

                JsonElement originalValue = property.Value;
                JsonValueKind originalValueKind = originalValue.ValueKind;

                if (newValueKind == JsonValueKind.Object && originalValueKind == JsonValueKind.Object)
                {
                    MergeObjects(jsonWriter, originalValue, newValue); // Recursive call
                }
                else if (newValueKind == JsonValueKind.Array && originalValueKind == JsonValueKind.Array)
                {
                    MergeArrays(jsonWriter, originalValue, newValue);
                }
                else
                {
                    newValue.WriteTo(jsonWriter);
                }
            }
            else
            {
                property.WriteTo(jsonWriter);
            }
        }

        // Write all the properties of the second document that are unique to it.
        foreach (JsonProperty property in root2.EnumerateObject())
        {
            if (!root1.TryGetProperty(property.Name, out _))
            {
                property.WriteTo(jsonWriter);
            }
        }

        jsonWriter.WriteEndObject();
    }

    private static void MergeArrays(Utf8JsonWriter jsonWriter, JsonElement root1, JsonElement root2)
    {

        jsonWriter.WriteStartArray();

        // Write all the elements from both JSON arrays
        foreach (JsonElement element in root1.EnumerateArray())
        {
            element.WriteTo(jsonWriter);
        }
        foreach (JsonElement element in root2.EnumerateArray())
        {
            element.WriteTo(jsonWriter);
        }

        jsonWriter.WriteEndArray();
    }

    #endregion

}