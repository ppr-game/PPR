using System.Collections.Generic;

using PER.Util;

namespace PER.Abstractions.Renderer;

public interface IEffect {
    public IEnumerable<PipelineStep>? pipeline { get; }
    public bool hasModifiers { get; }
    public bool drawable { get; }

    public (Vector2, RenderCharacter) ApplyModifiers(Vector2 position, RenderCharacter character);
    public void Update(bool fullscreen);
    public void Draw(Vector2Int position);

    public static void ApplyModifiers(IEffect? effect, ref (Vector2 position, RenderCharacter character) character) {
        if(effect is not null && effect.hasModifiers)
            character = effect.ApplyModifiers(character.position, character.character);
    }
}
