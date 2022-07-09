using System;
using System.IO;

using JetBrains.Annotations;

using PER.Abstractions.Resources;

namespace PRR.Resources;

[PublicAPI]
public class FontResource : IResource {
    public const string GlobalId = "graphics/font";

    public Font? font { get; private set; }

    public void Load(string id, IResources resources) {
        if(!resources.TryGetPath(Path.Join("graphics", "font", "font.png"), out string? imagePath) ||
            !resources.TryGetPath(Path.Join("graphics", "font", "mappings.txt"), out string? mappingsPath))
            throw new InvalidOperationException("Missing dependencies.");
        font = new Font(imagePath, mappingsPath);
    }

    public void Unload(string id, IResources resources) => font = null;
}
