using PER.Abstractions.Renderer;

using SFML.Graphics;

using BlendMode = SFML.Graphics.BlendMode;

namespace PRR.Sfml {
    public struct CachedPostProcessingStep {
        public PostProcessingStep.Type type { get; init; }
        public Shader shader { get; init; }
        public BlendMode blendMode { get; init; }
        public RenderStates renderState { get; init; }
    }
}
