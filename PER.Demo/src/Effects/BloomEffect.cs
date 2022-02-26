using System.Collections.Generic;
using System.IO;

using PER.Abstractions.Renderer;
using PER.Util;

namespace PER.Demo.Effects;

public class BloomEffect : IEffect {
    public IEnumerable<PipelineStep>? pipeline { get; }
    public bool hasModifiers => false;
    public bool drawable => false;

    public BloomEffect() {
        if(!Core.engine.resources.TryGetResource(Path.Join("graphics", "default_vert.glsl"), out string? vertexPath) ||
           !Core.engine.resources.TryGetResource(Path.Join("graphics", "bloom_frag.glsl"), out string? fragmentPath))
            return;

        pipeline = new[] {
            new PipelineStep {
                stepType = PipelineStep.Type.TemporaryText,
                blendMode = BlendMode.alpha
            },
            new PipelineStep {
                stepType = PipelineStep.Type.SwapBuffer
            },
            new PipelineStep {
                stepType = PipelineStep.Type.TemporaryScreen,
                vertexShader = File.ReadAllText(vertexPath),
                fragmentShader = File.ReadAllText(fragmentPath),
                blendMode = BlendMode.alpha
            },
            new PipelineStep {
                stepType = PipelineStep.Type.SwapBuffer
            },
            new PipelineStep {
                stepType = PipelineStep.Type.TemporaryScreen,
                vertexShader = File.ReadAllText(vertexPath),
                fragmentShader = File.ReadAllText(fragmentPath),
                blendMode = BlendMode.alpha
            },
            new PipelineStep {
                stepType = PipelineStep.Type.Screen,
                blendMode = BlendMode.max
            }
        };
    }

    public (Vector2, RenderCharacter) ApplyModifiers(Vector2 position, RenderCharacter character) =>
        (position, character);

    public void Update(bool fullscreen) { }

    public void Draw(Vector2Int position) { }
}
