using System.Linq;

using PER.Abstractions.Renderer;

using SFML.Graphics;

using BlendMode = SFML.Graphics.BlendMode;
using Shader = SFML.Graphics.Shader;

namespace PRR.Sfml {
    public class EffectContainer : IEffectContainer {
        public IEffect effect {
            get => _effect;
            set {
                _effect = value;
                pipeline = value?.pipeline?.Select(step => {
                    Shader shader = step.vertexShader is null || step.fragmentShader is null ? null :
                        Shader.FromString(step.vertexShader, null, step.fragmentShader);
                    BlendMode blendMode = SfmlConverters.ToSfmlBlendMode(step.blendMode);
                    CachedPipelineStep cachedStep = new() {
                        type = step.stepType,
                        shader = shader,
                        blendMode = blendMode,
                        renderState = new RenderStates(blendMode, Transform.Identity, null, shader)
                    };
                    return cachedStep;
                }).ToArray();
            }
        }
        
        public CachedPipelineStep[] pipeline { get; private set; }

        private IEffect _effect;
    }
}
