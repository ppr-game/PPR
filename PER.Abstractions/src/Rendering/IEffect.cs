using System.Collections.Generic;

using JetBrains.Annotations;

using PER.Util;

namespace PER.Abstractions.Rendering;

[PublicAPI]
public interface IEffect {
    public IEnumerable<PipelineStep>? pipeline { get; }
    public bool hasModifiers { get; }
    public bool drawable { get; }

    public void ApplyModifiers(Vector2Int at, ref Vector2 position, ref RenderCharacter character);
    public void Update(bool fullscreen);
    public void Draw(Vector2Int position);
}
