using System.Collections.Generic;

using PER.Util;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PER.Abstractions.Renderer {
    public interface IFont {
        IReadOnlyDictionary<(char, RenderStyle), Vector2[]> characters { get; }
        Vector2[] backgroundCharacter { get; }
        Vector2Int size { get; }
        Image<Rgba32> image { get; }
        string mappings { get; }
    }
}
