using System.Text.Json;

using PER.Abstractions.Resources;
using PER.Util;

namespace PER.Common.Resources;

public class ColorsResource : JsonResourceBase<IDictionary<string, Color>> {
    public const string GlobalId = "graphics/colors";

    public Dictionary<string, Color> colors { get; } = new();

    public override bool Load(string id, IResources resources) =>
        DeserializeAllJson(resources, Path.Join("graphics", "colors.json"), colors, () => false);

    protected override bool DeserializeJson(string path, IDictionary<string, Color> deserialized) {
        Dictionary<string, JsonElement>? elements =
            JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(File.ReadAllText(path));
        if(elements is null) return false;

        foreach((string? key, JsonElement element) in elements) {
            if(deserialized.ContainsKey(key)) continue;
            switch(element.ValueKind) {
                case JsonValueKind.Array: DeserializeArray(deserialized, element, key);
                    break;
                case JsonValueKind.String: DeserializeString(deserialized, element, key);
                    break;
                default: return false;
            }
        }

        return true;
    }

    private static void DeserializeArray(IDictionary<string, Color> currentValues, JsonElement element, string key) {
        int length = element.GetArrayLength();
        if(length is < 3 or > 4) return;
        currentValues.Add(key,
            new Color(element[0].GetByte(), element[1].GetByte(), element[2].GetByte(),
                length == 4 ? element[3].GetByte() : (byte)255));
    }

    private static void DeserializeString(IDictionary<string, Color> currentValues, JsonElement element, string key) {
        if(!currentValues.TryGetValue(element.GetString() ?? "", out Color color)) return;
        currentValues.Add(key, color);
    }

    public override bool Unload(string id, IResources resources) {
        colors.Clear();
        return true;
    }
}
