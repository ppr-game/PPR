using PER.Abstractions.Renderer;
using PER.Util;

namespace PER.Common.Effects;

public class DrawTextEffect : IEffect {
    public IEnumerable<PipelineStep>? pipeline { get; } = new[] {
        new PipelineStep {
            stepType = PipelineStep.Type.Text,
            blendMode = BlendMode.alpha
        }
    };
    public bool hasModifiers => false;
    public bool drawable => false;

    public (Vector2, RenderCharacter) ApplyModifiers(Vector2Int at, Vector2 position, RenderCharacter character) =>
        (position, character);

    public void Update(bool fullscreen) { }

    public void Draw(Vector2Int position) { }
}
