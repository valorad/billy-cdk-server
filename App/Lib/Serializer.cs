using System;
using System.Buffers;
using System.Text.Json;

namespace App.Lib
{
  public static class Serializer {
    public static T JSONToObject<T>(this JsonElement element, JsonSerializerOptions options = null)
    {
      // default: ignore key case
      if (options is null) {
        options = new JsonSerializerOptions() {
          PropertyNameCaseInsensitive = true
        };
      }
      var bufferWriter = new ArrayBufferWriter<byte>();
      using (var writer = new Utf8JsonWriter(bufferWriter))
          element.WriteTo(writer);
      return JsonSerializer.Deserialize<T>(bufferWriter.WrittenSpan, options);
    }

    public static T JSONToObject<T>(this JsonDocument document, JsonSerializerOptions options = null)
    {
      if (document == null)
          throw new ArgumentNullException(nameof(document));

      return document.RootElement.JSONToObject<T>(options);
    }
  }
}