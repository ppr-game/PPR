using System.Collections.Generic;

using JetBrains.Annotations;

using PER.Util;

namespace PER.Abstractions.Rendering;

[PublicAPI]
public interface IEffect {
    public IEnumerable<PipelineStep>? pipeline { get; }
    public bool hasModifiers { get; }
    public bool drawable { get; }

    public (Vector2, RenderCharacter) ApplyModifiers(Vector2Int at, Vector2 position, RenderCharacter character);
    public void Update(bool fullscreen);
    public void Draw(Vector2Int position);

    public static void ApplyModifiers(IEffect? effect, Vector2Int position,
        ref (Vector2 position, RenderCharacter character) character) {
        if(effect is not null && effect.hasModifiers)
            character = effect.ApplyModifiers(position, character.position, character.character);
    }
}
