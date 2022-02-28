using System.Text.Json;
using System.Text.Json.Serialization;

namespace PPR.Resources;

public class ColorsBytesConverter : JsonConverter<byte[]> {
    // AAAAAAAAAAAAAAAAAA
    public override byte[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        JsonDocument rootElement = JsonDocument.ParseValue(ref reader);
        return reader.TokenType == JsonTokenType.EndArray ?
            rootElement.RootElement.EnumerateArray().Select(element => element.GetByte()).ToArray() :
            Array.Empty<byte>();
    }

    public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, value, options);
}
