using System.Text.Json;

using JetBrains.Annotations;

using PER.Abstractions.Resources;
using PER.Util;

namespace PER.Common.Resources;

[PublicAPI]
public class ColorsResource : JsonResourceBase<IDictionary<string, (string?, Color)>> {
    public const string GlobalId = "graphics/colors";

    public Dictionary<string, Color> colors { get; } = new();

    public override void Load(string id, IResources resources) {
        Dictionary<string, (string?, Color)> tempValues = new();
        DeserializeAllJson(resources, Path.Combine("graphics", "colors.json"), tempValues, () => false);

        foreach((string key, (string? value, Color color)) in tempValues)
            if(value is null)
                colors.Add(key, color);

        foreach((string key, (string? value, Color _)) in tempValues)
            if(value is not null && colors.TryGetValue(value, out Color color))
                colors.Add(key, color);
    }

    protected override void DeserializeJson(string path, IDictionary<string, (string?, Color)> deserialized) {
        FileStream file = File.OpenRead(path);
        Dictionary<string, JsonElement>? elements = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(file);
        file.Close();

        if(elements is null)
            return;

        foreach((string? key, JsonElement element) in elements) {
            if(deserialized.ContainsKey(key)) continue;
            switch(element.ValueKind) {
                case JsonValueKind.Array: DeserializeArray(deserialized, element, key);
                    break;
                case JsonValueKind.String: DeserializeString(deserialized, element, key);
                    break;
                default:
                    throw new InvalidOperationException("Invalid color data.");
            }
        }
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

    public override void Unload(string id, IResources resources) => colors.Clear();
}
