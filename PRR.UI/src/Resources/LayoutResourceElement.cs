using System.Diagnostics.CodeAnalysis;

using PER.Abstractions.Audio;
using PER.Abstractions.Input;
using PER.Abstractions.Rendering;
using PER.Abstractions.Resources;
using PER.Abstractions.UI;
using PER.Util;

namespace PRR.UI.Resources;

[SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
public abstract class LayoutResourceElement {
    public bool? enabled { get; }
    public Vector2Int position { get; }
    public Vector2Int size { get; }

    protected LayoutResourceElement(bool? enabled, Vector2Int position, Vector2Int size) {
        this.enabled = enabled;
        this.position = position;
        this.size = size;
    }

    public abstract Element GetElement(IResources resources, IRenderer renderer, IInput input, IAudio audio,
        Dictionary<string, Color> colors, string layoutName, string id);

    protected static bool TryGetColor(Dictionary<string, Color> colors, string type, string layoutName, string id,
        string colorName, out Color color) =>
        colors.TryGetValue($"{type}_{layoutName}.{id}_{colorName}", out color) ||
        colors.TryGetValue($"{type}_@{id}_{colorName}", out color) ||
        colors.TryGetValue($"{type}_{colorName}", out color);
}
