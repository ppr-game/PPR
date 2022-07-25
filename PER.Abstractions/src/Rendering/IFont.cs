using System.Collections.Generic;

using JetBrains.Annotations;

using PER.Util;

namespace PER.Abstractions.Rendering;

[PublicAPI]
public interface IFont {
    public IReadOnlyDictionary<(char, RenderStyle), Vector2[]> characters { get; }
    public Vector2[] backgroundCharacter { get; }
    public Vector2Int size { get; }
    public Image image { get; }
    public string mappings { get; }

    public bool IsCharacterDrawable(char character, RenderStyle style);
}
