using JetBrains.Annotations;

using PER.Abstractions.Rendering;
using PER.Abstractions.Resources;
using PER.Util;

namespace PER.Common.Effects;

[PublicAPI]
public class BloomEffect : IEffect, IResource {
    public const string GlobalId = "graphics/effects/bloom";

    public IEnumerable<PipelineStep>? pipeline { get; private set; }
    public bool hasModifiers => false;
    public bool drawable => false;

    public void Load(string id, IResources resources) {
        if(!resources.TryGetPath(Path.Combine("graphics", "shaders", "default_vert.glsl"),
                out string? vertexPath) ||
            !resources.TryGetPath(Path.Combine("graphics", "shaders", "bloom_frag.glsl"),
                out string? fragmentPath) ||
            !resources.TryGetPath(Path.Combine("graphics", "shaders", "bloom-blend_frag.glsl"),
                out string? blendPath))
            throw new InvalidOperationException("Missing dependencies.");

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
                vertexShader = File.ReadAllText(vertexPath),
                fragmentShader = File.ReadAllText(blendPath),
                blendMode = BlendMode.alpha
            }
        };
    }

    public void Unload(string id, IResources resources) => pipeline = null;

    public void ApplyModifiers(Vector2Int at, ref Vector2 position, ref RenderCharacter character) { }

    public void Update(bool fullscreen) { }

    public void Draw(Vector2Int position) { }
}
