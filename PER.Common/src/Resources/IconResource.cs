using JetBrains.Annotations;

using PER.Abstractions.Resources;

namespace PER.Common.Resources;

[PublicAPI]
public class IconResource : Resource {
    public const string GlobalId = "graphics/icon";

    protected override IEnumerable<KeyValuePair<string, string>> paths { get; } = new Dictionary<string, string> {
        { "icon", "graphics/icon.png" }
    };

    public string? icon { get; private set; }

    public override void Load(string id) {
        if(TryGetPath("icon", out string? icon))
            this.icon = icon;
    }

    public override void Unload(string id) => icon = null;
}
