using System.Collections.Generic;

using PER.Util;

namespace PER.Abstractions.Renderer {
    public interface IEffect {
        public IEnumerable<PipelineStep> pipeline { get; }
        public bool hasModifiers { get; }
        public bool ended { get; }

        public (Vector2, RenderCharacter) ApplyModifiers(Vector2 position, RenderCharacter character);
    }
}
