using System.Collections.Generic;

using PER.Abstractions.Renderer;
using PER.Util;

namespace PER.Demo.Effects {
    public class DrawTextEffect : IEffect {
        public IEnumerable<PipelineStep> pipeline { get; } = new[] {
            new PipelineStep {
                stepType = PipelineStep.Type.Text,
                blendMode = BlendMode.alpha
            }
        };
        public bool hasModifiers => false;
        public bool ended => false;

        public (Vector2, RenderCharacter) ApplyModifiers(Vector2 position, RenderCharacter character) =>
            (position, character);
    }
}
