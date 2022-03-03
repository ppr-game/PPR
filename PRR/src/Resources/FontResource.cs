using System.IO;

using PER.Abstractions.Resources;

namespace PRR.Resources;

public class FontResource : IResource {
    public const string GlobalId = "graphics/font";

    public Font? font { get; private set; }

    public bool Load(string id, IResources resources) {
        if(!resources.TryGetPath(Path.Join("graphics", "font", "font.png"), out string? imagePath) ||
           !resources.TryGetPath(Path.Join("graphics", "font", "mappings.txt"), out string? mappingsPath))
            return false;
        font = new Font(imagePath, mappingsPath);
        return true;
    }

    public bool Unload(string id, IResources resources) {
        font = null;
        return true;
    }
}
