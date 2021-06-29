using System.Collections.Generic;
using System.IO;

using PER.Abstractions.Renderer;

namespace PER.Demo {
    public class BloomEffect : IEffect {
        public IEnumerable<PostProcessingStep> postProcessing { get; } = new[] {
            new PostProcessingStep {
                type = PostProcessingStep.Type.TemporaryText,
                blendMode = BlendMode.alpha
            },
            new PostProcessingStep {
                type = PostProcessingStep.Type.SwapBuffer
            },
            new PostProcessingStep {
                type = PostProcessingStep.Type.TemporaryScreen,
                vertexShader = File.ReadAllText(Path.Join("resources", "default_vert.glsl")),
                fragmentShader = File.ReadAllText(Path.Join("resources", "bloom_frag.glsl")),
                blendMode = BlendMode.alpha
            },
            new PostProcessingStep {
                type = PostProcessingStep.Type.SwapBuffer
            },
            new PostProcessingStep {
                type = PostProcessingStep.Type.TemporaryScreen,
                vertexShader = File.ReadAllText(Path.Join("resources", "default_vert.glsl")),
                fragmentShader = File.ReadAllText(Path.Join("resources", "bloom_frag.glsl")),
                blendMode = BlendMode.alpha
            },
            new PostProcessingStep {
                type = PostProcessingStep.Type.SwapBuffer
            },
            new PostProcessingStep {
                type = PostProcessingStep.Type.ClearBuffer
            },
            new PostProcessingStep {
                type = PostProcessingStep.Type.TemporaryText,
                blendMode = BlendMode.alpha
            },
            new PostProcessingStep {
                type = PostProcessingStep.Type.Screen,
                vertexShader = File.ReadAllText(Path.Join("resources", "default_vert.glsl")),
                fragmentShader = File.ReadAllText(Path.Join("resources", "blend-max_frag.glsl")),
                blendMode = BlendMode.alpha
            }
        };

        public bool ended => false;
    }
}
