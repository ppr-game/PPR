using PER.Abstractions.Resources;

namespace PER.Common.Resources;

public class IconResource : IResource {
    public const string GlobalId = "graphics/icon";

    public string? icon { get; private set; }

    public bool Load(string id, IResources resources) {
        resources.TryGetPath(Path.Join("graphics", "icon.png"), out string? icon);
        this.icon = icon;
        return true;
    }

    public bool Unload(string id, IResources resources) {
        icon = null;
        return true;
    }
}
