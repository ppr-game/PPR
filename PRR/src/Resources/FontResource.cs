using System;
using System.Collections.Generic;
using System.IO;

using JetBrains.Annotations;

using PER.Abstractions.Rendering;
using PER.Abstractions.Resources;

namespace PRR.Resources;

[PublicAPI]
public class FontResource : Resource {
    public const string GlobalId = "graphics/font";

    protected override IEnumerable<KeyValuePair<string, string>> paths { get; } = new Dictionary<string, string> {
        { "image", "graphics/font/font.qoi" },
        { "mappings", "graphics/font/mappings.txt" }
    };

    public Font? font { get; private set; }

    public override void Load(string id) {
        font = new Font(GetPath("image"), GetPath("mappings"));
    }

    public override void Unload(string id) => font = null;
}
