using System.Linq;

using PER.Abstractions.Renderer;

using Shader = SFML.Graphics.Shader;

namespace PRR {
    public class EffectContainer : IEffectContainer {
        public IEffect effect {
            get => _effect;
            set {
                _effect = value;
                if(value is null) {
                    textShader = null;
                    ppShaders = null;
                    return;
                }
                textShader = value.textShader.vertexShader is null && value.textShader.fragmentShader is null ? null :
                    Shader.FromString(value.textShader.vertexShader, null, value.textShader.fragmentShader);
                ppShaders = value.ppShaders?.Select(shader => Shader.FromString(shader.vertexShader, null,
                    shader.fragmentShader)).ToArray();
            }
        }
        
        public Shader textShader { get; private set; }
        public Shader[] ppShaders { get; private set; } // that's post processing shaders, ok, you pervert?

        private IEffect _effect;
    }
}
