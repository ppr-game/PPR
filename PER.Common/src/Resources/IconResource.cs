using JetBrains.Annotations;

using PER.Abstractions.Resources;

namespace PER.Common.Resources;

[PublicAPI]
public class IconResource : IResource {
    public const string GlobalId = "graphics/icon";

    public string? icon { get; private set; }

    public void Load(string id, IResources resources) {
        resources.TryGetPath(Path.Combine("graphics", "icon.png"), out string? icon);
        this.icon = icon;
    }

    public void Unload(string id, IResources resources) => icon = null;
}
