using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PPROld.Resources;

public class ColorsStringsConverter : JsonConverter<string> {
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        JsonDocument rootElement = JsonDocument.ParseValue(ref reader);
        return reader.TokenType == JsonTokenType.String ? rootElement.Deserialize<string>() ?? "" : "";
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, value, options);
}
