using System.Collections.Generic;
using System.IO;

using PER.Abstractions.Renderer;
using PER.Util;

namespace PER.Demo.Effects;

public class MaxBlendBloomEffect : IEffect {
    public IEnumerable<PipelineStep>? pipeline { get; } = new[] {
        new PipelineStep {
            stepType = PipelineStep.Type.TemporaryText,
            blendMode = BlendMode.alpha
        },
        new PipelineStep {
            stepType = PipelineStep.Type.SwapBuffer
        },
        new PipelineStep {
            stepType = PipelineStep.Type.TemporaryScreen,
            vertexShader = File.ReadAllText(Path.Join("resources", "default_vert.glsl")),
            fragmentShader = File.ReadAllText(Path.Join("resources", "bloom_frag.glsl")),
            blendMode = BlendMode.alpha
        },
        new PipelineStep {
            stepType = PipelineStep.Type.SwapBuffer
        },
        new PipelineStep {
            stepType = PipelineStep.Type.TemporaryScreen,
            vertexShader = File.ReadAllText(Path.Join("resources", "default_vert.glsl")),
            fragmentShader = File.ReadAllText(Path.Join("resources", "bloom_frag.glsl")),
            blendMode = BlendMode.alpha
        },
        new PipelineStep {
            stepType = PipelineStep.Type.SwapBuffer
        },
        new PipelineStep {
            stepType = PipelineStep.Type.ClearBuffer
        },
        new PipelineStep {
            stepType = PipelineStep.Type.TemporaryText,
            blendMode = BlendMode.alpha
        },
        new PipelineStep {
            stepType = PipelineStep.Type.Screen,
            vertexShader = File.ReadAllText(Path.Join("resources", "default_vert.glsl")),
            fragmentShader = File.ReadAllText(Path.Join("resources", "blend-max_frag.glsl")),
            blendMode = BlendMode.alpha
        }
    };

    public bool hasModifiers => false;
    public bool drawable => false;

    public (Vector2, RenderCharacter) ApplyModifiers(Vector2 position, RenderCharacter character) =>
        (position, character);

    public void Update(bool fullscreen) { }

    public void Draw(Vector2Int position) { }
}
