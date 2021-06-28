using System.IO;

using PER.Abstractions.Renderer;

namespace PER.Demo {
    public class BloomEffect : IEffect {
        public Shader textShader { get; } = new() { vertexShader = null, fragmentShader = null };
        public Shader[] ppShaders { get; } = {
            new() {
                vertexShader = File.ReadAllText(Path.Join("resources", "bloom_vert.glsl")),
                fragmentShader = File.ReadAllText(Path.Join("resources", "bloom_frag.glsl"))
            },
            new() {
                vertexShader = File.ReadAllText(Path.Join("resources", "bloom_vert.glsl")),
                fragmentShader = File.ReadAllText(Path.Join("resources", "bloom_frag.glsl"))
            }
        };
    }
}
