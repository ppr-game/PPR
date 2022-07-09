using System.Collections.Generic;

using JetBrains.Annotations;

using PER.Util;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PER.Abstractions.Rendering;

[PublicAPI]
public interface IFont {
    public IReadOnlyDictionary<(char, RenderStyle), Vector2[]> characters { get; }
    public Vector2[] backgroundCharacter { get; }
    public Vector2Int size { get; }
    public Image<Rgba32> image { get; }
    public string mappings { get; }

    public bool IsCharacterDrawable(char character, RenderStyle style);
}
