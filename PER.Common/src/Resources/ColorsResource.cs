using System.Text.Json;

using PER.Abstractions.Resources;
using PER.Util;

namespace PER.Common.Resources;

public class ColorsResource : JsonResourceBase<IDictionary<string, (string?, Color)>> {
    public const string GlobalId = "graphics/colors";

    public Dictionary<string, Color> colors { get; } = new();

    public override bool Load(string id, IResources resources) {
        Dictionary<string, (string?, Color)> tempValues = new();
        if(!DeserializeAllJson(resources, Path.Join("graphics", "colors.json"), tempValues, () => false)) return false;

        foreach((string key, (string? value, Color color)) in tempValues)
            if(value is null)
                colors.Add(key, color);

        foreach((string key, (string? value, Color _)) in tempValues)
            if(value is not null && colors.TryGetValue(value, out Color color))
                colors.Add(key, color);

        return true;
    }

    protected override bool DeserializeJson(string path, IDictionary<string, (string?, Color)> deserialized) {
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

    private static void DeserializeArray(IDictionary<string, (string?, Color)> currentValues, JsonElement element,
        string key) {
        int length = element.GetArrayLength();
        if(length is < 3 or > 4) return;
        currentValues.Add(key,
            (null, new Color(element[0].GetByte(), element[1].GetByte(), element[2].GetByte(),
                length == 4 ? element[3].GetByte() : (byte)255)));
    }

    private static void DeserializeString(IDictionary<string, (string?, Color)> currentValues, JsonElement element,
        string key) => currentValues.Add(key, (element.GetString() ?? "", new Color()));

    public override bool Unload(string id, IResources resources) {
        colors.Clear();
        return true;
    }
}
