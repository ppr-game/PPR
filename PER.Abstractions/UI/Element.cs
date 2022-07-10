using System.Collections.Generic;

using JetBrains.Annotations;

using PER.Abstractions.Audio;
using PER.Abstractions.Rendering;
using PER.Util;

namespace PER.Abstractions.UI;

[PublicAPI]
public abstract class Element {
    public IRenderer renderer { get; set; }
    public virtual bool enabled { get; set; } = true;
    public virtual Vector2Int position { get; set; }
    public virtual Vector2Int size { get; set; }

    public virtual Bounds bounds {
        get {
            Vector2Int position = this.position;
            Vector2Int size = this.size;
            return new Bounds(position, new Vector2Int(position.x + size.x - 1, position.y + size.y - 1));
        }
    }

    public virtual Vector2Int center =>
        new(position.x + (int)(size.x / 2f - 0.5f), position.y + (int)(size.y / 2f - 0.5f));

    public IEffect? effect { get; set; }

    protected Element(IRenderer renderer) => this.renderer = renderer;

    public abstract void Update(IReadOnlyStopwatch clock);

    protected static void PlaySound(IAudio? audio, IPlayable? playable, string defaultId) {
        if(playable is not null)
            playable.status = PlaybackStatus.Playing;
        else if(audio is not null && audio.TryGetPlayable(defaultId, out playable))
            playable.status = PlaybackStatus.Playing;
    }

    public abstract void UpdateColors(Dictionary<string, Color> colors, string layoutName, string id, string? special);

    protected static bool TryGetColor(Dictionary<string, Color> colors, string type, string layoutName, string id,
        string colorName, string? special, out Color color) =>
        special is not null && colors.TryGetValue($"{type}_{layoutName}.{id}_{colorName}_{special}", out color) ||
        special is not null && colors.TryGetValue($"{type}_@{id}_{colorName}_{special}", out color) ||
        special is not null && colors.TryGetValue($"{type}_{colorName}_{special}", out color) ||
        colors.TryGetValue($"{type}_{layoutName}.{id}_{colorName}", out color) ||
        colors.TryGetValue($"{type}_@{id}_{colorName}", out color) ||
        colors.TryGetValue($"{type}_{colorName}", out color);
}
