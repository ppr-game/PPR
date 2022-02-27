using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

using PER.Abstractions.Resources;
using PER.Util;

namespace PER.Demo.Resources;

public class ColorsResource : IResource {
    public Dictionary<string, Color> colors { get; } = new();

    public bool Load(string id, IResources resources) => resources.GetAllPaths(Path.Join("graphics", "colors.json"))
        .Aggregate(false, (current, colorsPath) => DeserializeColors(colorsPath, current, colors));

    private static bool DeserializeColors(string colorsPath, bool found, IDictionary<string, Color> currentValues) {
        Dictionary<string, byte[]>? values =
            JsonSerializer.Deserialize<Dictionary<string, byte[]>>(File.ReadAllText(colorsPath),
                new JsonSerializerOptions { Converters = { new ColorsBytesConverter() } });
        Dictionary<string, string>? variables =
            JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(colorsPath),
                new JsonSerializerOptions { Converters = { new ColorsStringsConverter() } });
        if(values is null) return found;
        foreach((string key, byte[] value) in values) {
            if(value.Length is < 3 or > 4) continue;
            found = true;
            currentValues[key] = new Color(value[0], value[1], value[2], value.Length == 4 ? value[3] : (byte)255);
        }

        if(variables is null) return found;
        foreach((string key, string value) in variables) {
            if(!currentValues.TryGetValue(value, out Color color)) continue;
            found = true;
            currentValues[key] = color;
        }

        return found;
    }

    public bool Unload(string id, IResources resources) {
        colors.Clear();
        return true;
    }
}
