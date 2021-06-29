using System.Collections.Generic;
using System.IO;

using PER.Abstractions.Renderer;

namespace PER.Demo {
    public class BloomEffect : IEffect {
        public IEnumerable<PostProcessingStep> postProcessing { get; } = new[] {
            new PostProcessingStep {
                stepType = PostProcessingStep.Type.TemporaryText,
                blendMode = BlendMode.alpha
            },
            new PostProcessingStep {
                stepType = PostProcessingStep.Type.SwapBuffer
            },
            new PostProcessingStep {
                stepType = PostProcessingStep.Type.TemporaryScreen,
                vertexShader = File.ReadAllText(Path.Join("resources", "default_vert.glsl")),
                fragmentShader = File.ReadAllText(Path.Join("resources", "bloom_frag.glsl")),
                blendMode = BlendMode.alpha
            },
            new PostProcessingStep {
                stepType = PostProcessingStep.Type.SwapBuffer
            },
            new PostProcessingStep {
                stepType = PostProcessingStep.Type.TemporaryScreen,
                vertexShader = File.ReadAllText(Path.Join("resources", "default_vert.glsl")),
                fragmentShader = File.ReadAllText(Path.Join("resources", "bloom_frag.glsl")),
                blendMode = BlendMode.alpha
            },
            new PostProcessingStep {
                stepType = PostProcessingStep.Type.SwapBuffer
            },
            new PostProcessingStep {
                stepType = PostProcessingStep.Type.ClearBuffer
            },
            new PostProcessingStep {
                stepType = PostProcessingStep.Type.TemporaryText,
                blendMode = BlendMode.alpha
            },
            new PostProcessingStep {
                stepType = PostProcessingStep.Type.Screen,
                vertexShader = File.ReadAllText(Path.Join("resources", "default_vert.glsl")),
                fragmentShader = File.ReadAllText(Path.Join("resources", "blend-max_frag.glsl")),
                blendMode = BlendMode.alpha
            }
        };

        public bool ended => false;
    }
}
