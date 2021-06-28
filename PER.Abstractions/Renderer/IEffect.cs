using PER.Util;

namespace PER.Abstractions.Renderer {
    public interface IEffect {
        public Shader textShader { get; }
        public Shader[] ppShaders { get; }
    }
}
